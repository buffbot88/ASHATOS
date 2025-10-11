using System;
using System.Linq;
using System.Threading.Tasks;
using Abstractions;
using ASHATCore.Modules.Extensions.Ashat;

namespace ASHATCore.Tests;

/// <summary>
/// Tests for ASHAT Personality, Emotion, and Relationship modules
/// Validates emotional intelligence and human relations features
/// </summary>
public class AshatPersonalityEmotionTests
{
    public static void RunTests()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ASHAT Personality & Emotion Intelligence Tests         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        TestPersonalityModule().Wait();
        TestEmotionModule().Wait();
        TestRelationshipModule().Wait();
        Testintegration().Wait();
        
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ✨ All Personality & Emotion Tests Passed! ✨          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    }

    #region Personality Module Tests

    private static async Task TestPersonalityModule()
    {
        Console.WriteLine("═══ Personality Module Tests ═══");
        Console.WriteLine();

        await TestPersonalityModule_GetDefaultPersonality();
        await TestPersonalityModule_SetPersonality();
        await TestPersonalityModule_GetTemplates();
        await TestPersonalityModule_CreateCustomPersonality();

        Console.WriteLine();
    }

    private static async Task TestPersonalityModule_GetDefaultPersonality()
    {
        Console.WriteLine("Test: Get Default Personality");
        
        var module = new AshatPersonalityModule();
        module.Initialize(null);
        var userId = "test_user_1";

        var config = await module.GetPersonalityConfigAsync(userId);

        if (config == null)
            throw new Exception("Config should not be null");
        if (config.Template != PersonalityTemplate.FriendlyAssistant)
            throw new Exception($"Expected FriendlyAssistant, got {config.Template}");
        if (config.CustomPersonality == null)
            throw new Exception("CustomPersonality should not be null");
        if (config.CustomPersonality.Name != "Friendly Assistant")
            throw new Exception($"Expected 'Friendly Assistant', got '{config.CustomPersonality.Name}'");

        Console.WriteLine("  ✅ Default personality returns Friendly Assistant");
        Console.WriteLine($"  ✅ Personality name: {config.CustomPersonality.Name}");
    }

    private static async Task TestPersonalityModule_SetPersonality()
    {
        Console.WriteLine("Test: Set Personality Configuration");
        
        var module = new AshatPersonalityModule();
        module.Initialize(null);
        var userId = "test_user_2";

        var customPersonality = new AshatPersonality
        {
            Name = "Test Personality",
            Openness = 0.9f,
            Conscientiousness = 0.8f,
            ExtASHATversion = 0.7f,
            Agreeableness = 0.85f,
            EmotionalStability = 0.9f
        };

        var config = new PersonalityConfiguration
        {
            UserId = userId,
            Template = PersonalityTemplate.Custom,
            CustomPersonality = customPersonality
        };

        var result = await module.SetPersonalityConfigAsync(userId, config);
        var retrieved = await module.GetPersonalityConfigAsync(userId);

        if (!result)
            throw new Exception("SetPersonalityConfigAsync should return true");
        if (retrieved == null)
            throw new Exception("Retrieved config should not be null");
        if (retrieved.CustomPersonality.Name != "Test Personality")
            throw new Exception($"Expected 'Test Personality', got '{retrieved.CustomPersonality.Name}'");
        if (Math.Abs(retrieved.CustomPersonality.Openness - 0.9f) > 0.001f)
            throw new Exception($"Expected Openness 0.9, got {retrieved.CustomPersonality.Openness}");

        Console.WriteLine("  ✅ Custom personality configured successfully");
        Console.WriteLine($"  ✅ Personality traits preserved: Openness={retrieved.CustomPersonality.Openness}");
    }

    private static async Task TestPersonalityModule_GetTemplates()
    {
        Console.WriteLine("Test: Get Personality Templates");
        
        var module = new AshatPersonalityModule();
        module.Initialize(null);

        var professional = await module.GetPersonalityTemplateAsync("professional");
        var playful = await module.GetPersonalityTemplateAsync("playful");
        var calm = await module.GetPersonalityTemplateAsync("calm");
        var templates = await module.GetAllTemplatesAsync();

        if (professional == null)
            throw new Exception("Professional template should not be null");
        if (professional.Name != "Professional Mentor")
            throw new Exception($"Expected 'Professional Mentor', got '{professional.Name}'");
        if (playful == null || playful.Name != "Playful Companion")
            throw new Exception("Playful template incorrect");
        if (calm == null || calm.Name != "Calm Guide")
            throw new Exception("Calm template incorrect");
        if (templates.Count < 6)
            throw new Exception($"Expected at least 6 templates, got {templates.Count}");

        Console.WriteLine("  ✅ All personality templates available");
        Console.WriteLine($"  ✅ Total templates: {templates.Count}");
    }

    private static async Task TestPersonalityModule_CreateCustomPersonality()
    {
        Console.WriteLine("Test: Create Custom Personality");
        
        var module = new AshatPersonalityModule();
        module.Initialize(null);

        var personality = new AshatPersonality
        {
            Name = "Custom Test",
            Description = "A test personality",
            Openness = 0.75f
        };

        var created = await module.CreateCustomPersonalityAsync(personality);

        if (created == null)
            throw new Exception("Created personality should not be null");
        if (string.IsNullOrEmpty(created.PersonalityId))
            throw new Exception("PersonalityId should be Generated");
        if (created.CreatedAt == default(DateTime))
            throw new Exception("CreatedAt should be set");
        if (created.Name != "Custom Test")
            throw new Exception($"Expected 'Custom Test', got '{created.Name}'");

        Console.WriteLine("  ✅ Custom personality created with unique ID");
        Console.WriteLine($"  ✅ Personality ID: {created.PersonalityId.Substring(0, 8)}...");
    }

    #endregion

    #region Emotion Module Tests

    private static async Task TestEmotionModule()
    {
        Console.WriteLine("═══ Emotion Module Tests ═══");
        Console.WriteLine();

        await TestEmotionModule_GetDefaultEmotionalState();
        await TestEmotionModule_SetEmotionalState();
        await TestEmotionModule_DetectUserEmotion();
        await TestEmotionModule_GenerateResponse();

        Console.WriteLine();
    }

    private static async Task TestEmotionModule_GetDefaultEmotionalState()
    {
        Console.WriteLine("Test: Get Default Emotional State");
        
        var module = new AshatEmotionModule();
        module.Initialize(null);
        var userId = "emotion_test_1";

        var state = await module.GetEmotionalStateAsync(userId);

        if (state == null)
            throw new Exception("State should not be null");
        if (state.CurrentEmotion != EmotionType.Supportive)
            throw new Exception($"Expected Supportive emotion, got {state.CurrentEmotion}");
        if (state.Valence <= 0.5f)
            throw new Exception($"Expected positive valence, got {state.Valence}");

        Console.WriteLine("  ✅ Default emotional state is Supportive and positive");
        Console.WriteLine($"  ✅ Valence: {state.Valence:P0}, Arousal: {state.Arousal:P0}");
    }

    private static async Task TestEmotionModule_SetEmotionalState()
    {
        Console.WriteLine("Test: Set Emotional State");
        
        var module = new AshatEmotionModule();
        module.Initialize(null);
        var userId = "emotion_test_2";

        var newState = new AshatEmotionalState
        {
            CurrentEmotion = EmotionType.Excited,
            Intensity = 0.9f,
            Context = "User achieved milestone",
            Valence = 0.95f,
            Arousal = 0.9f
        };

        var result = await module.SetEmotionalStateAsync(userId, newState);
        var retrieved = await module.GetEmotionalStateAsync(userId);

        if (!result)
            throw new Exception("SetEmotionalStateAsync should return true");
        if (retrieved.CurrentEmotion != EmotionType.Excited)
            throw new Exception($"Expected Excited, got {retrieved.CurrentEmotion}");
        if (Math.Abs(retrieved.Intensity - 0.9f) > 0.001f)
            throw new Exception($"Expected intensity 0.9, got {retrieved.Intensity}");

        Console.WriteLine("  ✅ Emotional state updated successfully");
        Console.WriteLine($"  ✅ Context preserved: {retrieved.Context}");
    }

    private static async Task TestEmotionModule_DetectUserEmotion()
    {
        Console.WriteLine("Test: Detect User Emotion");
        
        var module = new AshatEmotionModule();
        module.Initialize(null);
        var userId = "emotion_test_3";

        var excited = await module.DetectUserEmotionAsync(userId, "Wow! This is amazing!");
        var Grateful = await module.DetectUserEmotionAsync(userId, "Thank you so much for your help!");

        if (excited == null || excited.DetectedEmotion != EmotionType.Excited)
            throw new Exception($"Expected Excited emotion detection");
        if (excited.Confidence <= 0.5f)
            throw new Exception($"Expected high confidence, got {excited.Confidence}");
        if (Grateful == null || Grateful.DetectedEmotion != EmotionType.Grateful)
            throw new Exception($"Expected Grateful emotion detection");

        Console.WriteLine("  ✅ User emotions detected accuRately");
        Console.WriteLine($"  ✅ Excited: {excited.Confidence:P0}, Grateful: {Grateful.Confidence:P0}");
    }

    private static async Task TestEmotionModule_GenerateResponse()
    {
        Console.WriteLine("Test: Generate Emotional Response");
        
        var module = new AshatEmotionModule();
        module.Initialize(null);
        var userId = "emotion_test_4";

        var excitedResponse = await module.GenerateResponseAsync(userId, EmotionType.Excited);
        var GratefulResponse = await module.GenerateResponseAsync(userId, EmotionType.Grateful);

        if (excitedResponse == null)
            throw new Exception("Excited response should not be null");
        if (excitedResponse.ResponseEmotion != EmotionType.Excited)
            throw new Exception($"Expected Excited response emotion");
        if (!excitedResponse.ResponseText.ToLower().Contains("excit"))
            throw new Exception("Response should mention excitement");
        if (GratefulResponse == null || GratefulResponse.ResponseEmotion != EmotionType.Grateful)
            throw new Exception("Grateful response incorrect");

        Console.WriteLine("  ✅ Empathetic responses Generated successfully");
        Console.WriteLine($"  ✅ Supportive actions included: {excitedResponse.SupportiveActions.Length}");
    }

    #endregion

    #region Relationship Module Tests

    private static async Task TestRelationshipModule()
    {
        Console.WriteLine("═══ Relationship Module Tests ═══");
        Console.WriteLine();

        await TestRelationshipModule_GetRelationship();
        await TestRelationshipModule_RecordInteraction();
        await TestRelationshipModule_LearnPreference();
        await TestRelationshipModule_ProvideReinforcement();

        Console.WriteLine();
    }

    private static async Task TestRelationshipModule_GetRelationship()
    {
        Console.WriteLine("Test: Get User Relationship");
        
        var module = new AshatRelationshipModule();
        module.Initialize(null);
        var userId = "relationship_test_1";

        var relationship = await module.GetRelationshipAsync(userId);

        if (relationship == null)
            throw new Exception("Relationship should not be null");
        if (relationship.UserId != userId)
            throw new Exception($"Expected userId {userId}, got {relationship.UserId}");
        if (relationship.Level != RelationshipLevel.New)
            throw new Exception($"Expected New level, got {relationship.Level}");
        if (relationship.Milestones.Count < 1)
            throw new Exception("Should have at least first Interaction milestone");

        Console.WriteLine("  ✅ New relationship created successfully");
        Console.WriteLine($"  ✅ Starting level: {relationship.Level}");
    }

    private static async Task TestRelationshipModule_RecordInteraction()
    {
        Console.WriteLine("Test: Record Interactions");
        
        var module = new AshatRelationshipModule();
        module.Initialize(null);
        var userId = "relationship_test_2";

        var result1 = await module.RecordInteractionAsync(userId, positive: true);
        var result2 = await module.RecordInteractionAsync(userId, positive: true);
        var relationship = await module.GetRelationshipAsync(userId);

        if (!result1 || !result2)
            throw new Exception("RecordInteractionAsync should return true");
        if (relationship.InteractionCount != 2)
            throw new Exception($"Expected 2 Interactions, got {relationship.InteractionCount}");
        if (relationship.PositiveInteractions != 2)
            throw new Exception($"Expected 2 positive Interactions, got {relationship.PositiveInteractions}");
        if (relationship.TrustScore <= 0.5f)
            throw new Exception($"Trust should increase, got {relationship.TrustScore}");

        Console.WriteLine("  ✅ Interactions recorded and metrics updated");
        Console.WriteLine($"  ✅ Trust score increased to: {relationship.TrustScore:P0}");
    }

    private static async Task TestRelationshipModule_LearnPreference()
    {
        Console.WriteLine("Test: Learn User Preference");
        
        var module = new AshatRelationshipModule();
        module.Initialize(null);
        var userId = "relationship_test_3";

        var result = await module.LearnPreferenceAsync(userId, "likes_humor", "true");
        var relationship = await module.GetRelationshipAsync(userId);

        if (!result)
            throw new Exception("LearnPreferenceAsync should return true");
        if (!relationship.LearnedPreferences.ContainsKey("likes_humor"))
            throw new Exception("Preference should be stored");
        if (relationship.LearnedPreferences["likes_humor"] != "true")
            throw new Exception($"Expected 'true', got '{relationship.LearnedPreferences["likes_humor"]}'");

        Console.WriteLine("  ✅ User preference learned successfully");
        Console.WriteLine($"  ✅ Total preferences: {relationship.LearnedPreferences.Count}");
    }

    private static async Task TestRelationshipModule_ProvideReinforcement()
    {
        Console.WriteLine("Test: Provide Positive Reinforcement");
        
        var module = new AshatRelationshipModule();
        module.Initialize(null);
        var userId = "relationship_test_4";

        var reinforcement = await module.ProvideReinforcementAsync(userId, "completed first project");

        if (reinforcement == null)
            throw new Exception("Reinforcement should not be null");
        if (reinforcement.UserId != userId)
            throw new Exception($"Expected userId {userId}");
        if (reinforcement.Achievement != "completed first project")
            throw new Exception("Achievement text incorrect");
        if (string.IsNullOrEmpty(reinforcement.Message))
            throw new Exception("Message should be Generated");

        Console.WriteLine("  ✅ Positive reinforcement provided");
        Console.WriteLine($"  ✅ Type: {reinforcement.Type}, Message: {reinforcement.Message.Substring(0, Math.Min(50, reinforcement.Message.Length))}...");
    }

    #endregion

    #region integration Tests

    private static async Task Testintegration()
    {
        Console.WriteLine("═══ integration Tests ═══");
        Console.WriteLine();

        await Testintegration_AllModulesTogether();

        Console.WriteLine();
    }

    private static async Task Testintegration_AllModulesTogether()
    {
        Console.WriteLine("Test: All Three Modules Working Together");
        
        var personalityModule = new AshatPersonalityModule();
        var emotionModule = new AshatEmotionModule();
        var relationshipModule = new AshatRelationshipModule();
        
        personalityModule.Initialize(null);
        emotionModule.Initialize(null);
        relationshipModule.Initialize(null);
        
        var userId = "integration_test_1";

        // Set personality
        var mentorTemplate = await personalityModule.GetPersonalityTemplateAsync("professional");
        if (mentorTemplate == null)
            throw new Exception("Mentor template should not be null");
            
        var personalityConfig = new PersonalityConfiguration
        {
            UserId = userId,
            Template = PersonalityTemplate.ProfessionalMentor,
            CustomPersonality = mentorTemplate
        };
        await personalityModule.SetPersonalityConfigAsync(userId, personalityConfig);

        // Set emotion
        var emotionState = new AshatEmotionalState
        {
            CurrentEmotion = EmotionType.encouraging,
            Intensity = 0.8f
        };
        await emotionModule.SetEmotionalStateAsync(userId, emotionState);

        // Build relationship
        await relationshipModule.RecordInteractionAsync(userId, positive: true);
        await relationshipModule.LearnPreferenceAsync(userId, "prefers_formal", "true");

        // Verify all three modules maintain independent state
        var personality = await personalityModule.GetPersonalityConfigAsync(userId);
        var emotion = await emotionModule.GetEmotionalStateAsync(userId);
        var relationship = await relationshipModule.GetRelationshipAsync(userId);

        if (personality.CustomPersonality.Name != "Professional Mentor")
            throw new Exception("Personality not preserved");
        if (emotion.CurrentEmotion != EmotionType.encouraging)
            throw new Exception("Emotion not preserved");
        if (!relationship.LearnedPreferences.ContainsKey("prefers_formal"))
            throw new Exception("Preference not preserved");

        Console.WriteLine("  ✅ All three modules work together seamlessly");
        Console.WriteLine($"  ✅ Personality: {personality.CustomPersonality.Name}");
        Console.WriteLine($"  ✅ Emotion: {emotion.CurrentEmotion}");
        Console.WriteLine($"  ✅ Relationship Level: {relationship.Level}");
    }

    #endregion
}
