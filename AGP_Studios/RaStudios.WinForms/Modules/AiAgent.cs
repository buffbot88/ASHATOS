using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaStudios.WinForms.Models;

namespace RaStudios.WinForms.Modules
{
    /// <summary>
    /// AI Coding Bot integration with human-in-the-loop security controls.
    /// Provides code generation, snippet creation, and CI automation with explicit approval requirements.
    /// </summary>
    public class AiAgent : IDisposable
    {
        private readonly ServerConnector serverConnector;
        private readonly HttpClient httpClient;
        private string aiEndpoint = "http://localhost:8088/api/ai/generate";
        private readonly SemaphoreSlim rateLimiter;
        private readonly int maxRequestsPerMinute = 10;
        private readonly Queue<DateTime> requestTimestamps = new Queue<DateTime>();
        private bool isDisposed = false;

        public event EventHandler<string>? OnStatusUpdate;
        public event EventHandler<CodeGenerationResult>? OnCodeGenerated;
        public event EventHandler<string>? OnError;

        public string AiEndpoint
        {
            get => aiEndpoint;
            set => aiEndpoint = value;
        }

        public AiAgent(ServerConnector connector)
        {
            serverConnector = connector ?? throw new ArgumentNullException(nameof(connector));
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            rateLimiter = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Generates code snippet based on prompt with human approval required.
        /// </summary>
        public async Task<CodeGenerationResult?> GenerateCodeAsync(string prompt, string language = "csharp")
        {
            try
            {
                // Rate limiting check
                if (!await CheckRateLimitAsync())
                {
                    RaiseError("Rate limit exceeded. Please wait before making another request.");
                    return null;
                }

                var promptPreview = prompt?.Length > 50 ? prompt.Substring(0, 50) + "..." : prompt;
                RaiseStatusUpdate($"Generating {language} code for prompt: {promptPreview}");

                // Validate prompt for safety
                if (!ValidatePrompt(prompt))
                {
                    RaiseError("Prompt contains potentially unsafe content and was rejected.");
                    return null;
                }

                var request = new CodeGenerationRequest
                {
                    Prompt = prompt,
                    Language = language,
                    MaxTokens = 1000,
                    Temperature = 0.7,
                    RequireHumanApproval = true // Always require human approval
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(aiEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    RaiseError($"AI API request failed: {response.StatusCode}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CodeGenerationResult>(responseJson);

                if (result != null)
                {
                    result.RequiresHumanApproval = true;
                    result.IsApproved = false; // Must be explicitly approved
                    result.GeneratedAt = DateTime.UtcNow;

                    RaiseStatusUpdate("Code generated successfully. Awaiting human approval.");
                    OnCodeGenerated?.Invoke(this, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                RaiseError($"Code generation error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Approves generated code for use. MUST be called by human operator.
        /// </summary>
        public bool ApproveCode(CodeGenerationResult codeResult, string approverName)
        {
            if (codeResult == null)
                throw new ArgumentNullException(nameof(codeResult));

            if (string.IsNullOrWhiteSpace(approverName))
                throw new ArgumentException("Approver name is required for audit trail.");

            // Perform final validation
            if (!ValidateGeneratedCode(codeResult.GeneratedCode))
            {
                RaiseError("Generated code failed final validation.");
                return false;
            }

            codeResult.IsApproved = true;
            codeResult.ApprovedBy = approverName;
            codeResult.ApprovedAt = DateTime.UtcNow;

            RaiseStatusUpdate($"Code approved by {approverName} and ready for deployment.");
            return true;
        }

        /// <summary>
        /// Validates prompt for potentially dangerous patterns.
        /// </summary>
        private bool ValidatePrompt(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return false;

            // Block common dangerous patterns
            var dangerousPatterns = new[]
            {
                "rm -rf", "del /f", "format c:",
                "drop database", "delete from",
                "__import__('os').system",
                "eval(", "exec(",
                "Process.Start",
                // Add more patterns as needed
            };

            var lowerPrompt = prompt.ToLowerInvariant();
            foreach (var pattern in dangerousPatterns)
            {
                if (lowerPrompt.Contains(pattern.ToLowerInvariant()))
                {
                    RaiseStatusUpdate($"Blocked dangerous pattern: {pattern}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates generated code before approval.
        /// </summary>
        private bool ValidateGeneratedCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            // Basic validation - can be extended with static analysis
            var dangerousPatterns = new[]
            {
                "System.Diagnostics.Process.Start",
                "File.Delete(",
                "Directory.Delete(",
                "Registry.SetValue",
                "unsafe {",
                // Add more patterns as needed
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (code.Contains(pattern))
                {
                    RaiseStatusUpdate($"Warning: Generated code contains {pattern}");
                    // Don't automatically reject, but warn the user
                }
            }

            return true;
        }

        /// <summary>
        /// Checks and enforces rate limiting.
        /// </summary>
        private async Task<bool> CheckRateLimitAsync()
        {
            await rateLimiter.WaitAsync();
            try
            {
                var now = DateTime.UtcNow;
                var oneMinuteAgo = now.AddMinutes(-1);

                // Remove old timestamps
                while (requestTimestamps.Count > 0 && requestTimestamps.Peek() < oneMinuteAgo)
                {
                    requestTimestamps.Dequeue();
                }

                if (requestTimestamps.Count >= maxRequestsPerMinute)
                {
                    return false;
                }

                requestTimestamps.Enqueue(now);
                return true;
            }
            finally
            {
                rateLimiter.Release();
            }
        }

        /// <summary>
        /// Deploys approved code to the game server. Requires explicit confirmation.
        /// </summary>
        public async Task<bool> DeployCodeAsync(CodeGenerationResult codeResult, bool confirmedByHuman)
        {
            if (!confirmedByHuman)
            {
                RaiseError("Code deployment requires explicit human confirmation.");
                return false;
            }

            if (codeResult == null || !codeResult.IsApproved)
            {
                RaiseError("Code must be approved before deployment.");
                return false;
            }

            if (!serverConnector.IsConnected)
            {
                RaiseError("Cannot deploy code: Not connected to server.");
                return false;
            }

            try
            {
                RaiseStatusUpdate("Deploying approved code to server...");

                var deploymentMessage = new
                {
                    type = "code_deployment",
                    code = codeResult.GeneratedCode,
                    language = codeResult.Language,
                    approver = codeResult.ApprovedBy,
                    timestamp = DateTime.UtcNow
                };

                await serverConnector.SendMessageAsync(JsonConvert.SerializeObject(deploymentMessage));
                
                RaiseStatusUpdate("Code deployed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                RaiseError($"Deployment failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Requests code snippet for specific functionality.
        /// </summary>
        public async Task<string?> RequestSnippetAsync(string functionality, string context = "")
        {
            var prompt = $"Generate a code snippet for: {functionality}";
            if (!string.IsNullOrEmpty(context))
            {
                prompt += $"\nContext: {context}";
            }

            var result = await GenerateCodeAsync(prompt);
            return result?.GeneratedCode;
        }

        private void RaiseStatusUpdate(string message)
        {
            OnStatusUpdate?.Invoke(this, message);
        }

        private void RaiseError(string message)
        {
            OnError?.Invoke(this, message);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                httpClient?.Dispose();
                rateLimiter?.Dispose();
                isDisposed = true;
            }
        }
    }
}
