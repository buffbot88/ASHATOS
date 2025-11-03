# AI Chatbot for LegendaryCMS - Quick Start Guide

## Overview

The AI Chatbot feature provides intelligent, context-aware chat support for users of the LegendaryCMS platform. It can answer questions about content management, forums, blogs, permissions, and general CMS operations.

**✨ Key Features:**
- 🤖 AI-powered intelligent responses using language models
- 💬 Context-aware conversations with history tracking
- 🔄 Fallback responses when AI unavailable
- 👥 Multi-user conversation support
- 📊 Usage statistics and monitoring
- 🔒 User-authenticated sessions

---

## Quick Start

### 1. Initialize the CMS with Chatbot

The AI Chatbot is automatically initialized when you load the LegendaryCMS module:

```bash
cd RaCore
dotnet run
```

You should see in the console:
```
[LegendaryCMS] AI Chatbot system initialized
[AIChatbot] Initialized - AI Module: Available/Unavailable
```

### 2. Check Chatbot Status

```bash
> cms chatbot
```

**Output:**
```
AI Chatbot Information:

Status:
  • AI Module: ✓ Available
  • Bot Name: RaBot
  • Active Conversations: 0
  • Total Conversations: 0
  • Total Messages: 0

API Endpoints:
  • POST /api/chatbot/start - Start a new conversation
  • POST /api/chatbot/message - Send a message to the bot
  • GET /api/chatbot/history - Get conversation history
  • GET /api/chatbot/conversations - Get all user conversations
  • POST /api/chatbot/end - End a conversation
  • GET /api/chatbot/stats - Get chatbot statistics
```

---

## API Usage

### Start a Conversation

**Endpoint:** `POST /api/chatbot/start`

**Request:**
```json
{
  "username": "john_doe"
}
```

**Response:**
```json
{
  "statusCode": 200,
  "data": {
    "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    "userId": "user123",
    "username": "john_doe",
    "startedAt": "2024-01-15T10:30:00Z",
    "lastActivityAt": "2024-01-15T10:30:00Z",
    "messageCount": 0,
    "isActive": true
  }
}
```

---

### Send a Message

**Endpoint:** `POST /api/chatbot/message`

**Request:**
```json
{
  "conversationId": "550e8400-e29b-41d4-a716-446655440000",
  "message": "How do I create a new forum?"
}
```

**Response:**
```json
{
  "statusCode": 200,
  "data": {
    "success": true,
    "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    "botReply": "To create a forum, use the /api/forums/post endpoint with proper authentication. You'll need the ForumPost permission...",
    "messageId": "660e8400-e29b-41d4-a716-446655440001",
    "timestamp": "2024-01-15T10:31:00Z"
  }
}
```

---

### Get Conversation History

**Endpoint:** `GET /api/chatbot/history?conversationId={id}&limit=50`

**Response:**
```json
{
  "statusCode": 200,
  "data": {
    "conversationId": "550e8400-e29b-41d4-a716-446655440000",
    "messages": [
      {
        "messageId": "...",
        "conversationId": "...",
        "content": "How do I create a new forum?",
        "isFromBot": false,
        "timestamp": "2024-01-15T10:31:00Z"
      },
      {
        "messageId": "...",
        "conversationId": "...",
        "content": "To create a forum, use the /api/forums/post endpoint...",
        "isFromBot": true,
        "timestamp": "2024-01-15T10:31:01Z"
      }
    ]
  }
}
```

---

### Get User Conversations

**Endpoint:** `GET /api/chatbot/conversations`

**Response:**
```json
{
  "statusCode": 200,
  "data": {
    "userId": "user123",
    "conversations": [
      {
        "conversationId": "...",
        "userId": "user123",
        "username": "john_doe",
        "startedAt": "2024-01-15T10:30:00Z",
        "lastActivityAt": "2024-01-15T10:35:00Z",
        "messageCount": 6,
        "isActive": true
      }
    ]
  }
}
```

---

### End a Conversation

**Endpoint:** `POST /api/chatbot/end`

**Request:**
```json
{
  "conversationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response:**
```json
{
  "statusCode": 200,
  "data": {
    "message": "Conversation ended successfully"
  }
}
```

---

### Get Chatbot Statistics

**Endpoint:** `GET /api/chatbot/stats`

**Response:**
```json
{
  "statusCode": 200,
  "data": {
    "activeConversations": 5,
    "totalConversations": 42,
    "totalMessages": 256,
    "aiModuleAvailable": true,
    "botName": "RaBot"
  }
}
```

---

## Common Use Cases

### 1. Help with Content Management

**User:** "How do I publish a blog post?"

**RaBot:** "Blogs are a great way to share content. Use the /api/blogs/create endpoint to publish blog posts. You'll need appropriate permissions to create blogs."

### 2. Understanding Permissions

**User:** "What permissions do I need to moderate forums?"

**RaBot:** "The CMS uses Role-Based Access Control (RBAC) with multiple permission levels. For forum moderation, you need the forum.moderate permission. Contact your administrator to adjust your permissions."

### 3. API Information

**User:** "What API endpoints are available?"

**RaBot:** "The CMS provides a comprehensive REST API with endpoints for forums, blogs, content, and more. Check /api/endpoints for a full list of available APIs."

---

## AI Integration

The chatbot integrates with the Language Model Processor when available:

### With AI Module
- Context-aware responses based on conversation history
- Natural language understanding
- Personalized assistance
- Advanced query handling

### Without AI Module (Fallback Mode)
- Pattern-matching responses
- Predefined answers for common questions
- Basic help with CMS features
- Still fully functional for standard queries

---

## Authentication

All chatbot endpoints require authentication except `/api/chatbot/stats`.

**Authentication Headers:**
```
Authorization: Bearer <your-token>
```

The chatbot automatically associates conversations with the authenticated user's ID.

---

## Best Practices

### For Users

1. **Start with Clear Questions**: "How do I create a forum?" works better than "forums?"
2. **Provide Context**: If asking follow-up questions, reference previous topics
3. **Be Specific**: "What permissions do I need to post?" vs. "permissions?"
4. **End Conversations**: Use the end endpoint when done to free resources

### For Developers

1. **Handle Errors**: Check `response.success` before using bot replies
2. **Respect Rate Limits**: The API has rate limiting enabled
3. **Store Conversation IDs**: Cache conversation IDs for ongoing sessions
4. **Check AI Availability**: Use `/api/chatbot/stats` to check if AI module is available

---

## Configuration

The chatbot can be configured in `cms-config.json`:

```json
{
  "Chatbot": {
    "Enabled": true,
    "MaxConversationHistoryLength": 50,
    "BotName": "RaBot",
    "DefaultLanguage": "en"
  }
}
```

---

## Troubleshooting

### Bot Not Responding

**Check:**
1. Is the LegendaryCMS module loaded? (`cms status`)
2. Is authentication valid? (401 = unauthorized)
3. Is the conversation ID correct?

### Generic Responses Only

**Cause:** AI Language Module not available

**Solution:** The chatbot uses fallback responses. To enable AI:
1. Ensure Language Model Processor is loaded
2. Check if .gguf model files are present
3. Verify AI module initialization in logs

### Rate Limit Errors

**Cause:** Too many requests in short time

**Solution:** 
- Wait a minute before retrying
- Implement request throttling in your client
- Contact admin for rate limit adjustments

---

## Testing

Run the chatbot tests:

```bash
cd ASHATCore
dotnet test --filter "AIChatbotTests"
```

Or use the test runner:
```bash
> test-runner start chatbot
```

---

## Example Client Integration

### JavaScript/TypeScript

```typescript
class ChatbotClient {
  private baseUrl = 'http://localhost:8080/api/chatbot';
  private token: string;
  private conversationId?: string;

  constructor(token: string) {
    this.token = token;
  }

  async startConversation(username: string) {
    const response = await fetch(`${this.baseUrl}/start`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ username })
    });
    
    const data = await response.json();
    this.conversationId = data.data.conversationId;
    return data;
  }

  async sendMessage(message: string) {
    if (!this.conversationId) {
      throw new Error('No active conversation');
    }

    const response = await fetch(`${this.baseUrl}/message`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        conversationId: this.conversationId,
        message
      })
    });
    
    return await response.json();
  }

  async getHistory(limit = 50) {
    const response = await fetch(
      `${this.baseUrl}/history?conversationId=${this.conversationId}&limit=${limit}`,
      {
        headers: {
          'Authorization': `Bearer ${this.token}`
        }
      }
    );
    
    return await response.json();
  }
}

// Usage
const chatbot = new ChatbotClient('your-token');
await chatbot.startConversation('john_doe');
const response = await chatbot.sendMessage('How do I create a blog?');
console.log(response.data.botReply);
```

---

## Security Considerations

1. **Authentication Required**: All endpoints (except stats) require authentication
2. **User Isolation**: Users can only access their own conversations
3. **Rate Limiting**: Prevents abuse and DoS attacks
4. **Input Validation**: All inputs are validated and sanitized
5. **No PII Storage**: Personal information is not logged or stored long-term

---

## Support

For issues or questions:

1. Check this documentation
2. Review the console logs (`cms chatbot`)
3. Test with simple queries first
4. Verify API endpoints are registered (`cms api`)
5. Check authentication tokens

---

## Future Enhancements

Planned features:
- [ ] Multi-language support
- [ ] Custom bot personalities
- [ ] Integration with CMS knowledge base
- [ ] Voice input/output
- [ ] Sentiment analysis
- [ ] Conversation analytics dashboard
- [ ] Plugin system for custom responses

---

**Last Updated:** 2024-01-15  
**Version:** 8.0.0  
**Module:** LegendaryCMS  
**Status:** ✅ Production Ready
