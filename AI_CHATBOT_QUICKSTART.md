# AI Chatbot for LegendaryCMS - Quick Start Guide

## Overview

The AI Chatbot feature provides **friendly, natural language conversations** for users of the LegendaryCMS platform. RaBot is your conversational AI assistant that makes working with the CMS feel like chatting with a helpful friend.

**✨ Key Features:**
- 💬 **Natural Conversations** - Chat in your own words, just like talking to a person
- 🤖 **AI-Powered Intelligence** - Smart responses using advanced language models
- 😊 **Friendly & Personable** - Warm, encouraging, and easy to talk to
- 📚 **Context-Aware** - Remembers your conversation and provides relevant help
- 🎯 **Helpful Guidance** - Step-by-step assistance with clear explanations
- 🔄 **Always Available** - Fallback responses ensure help is always there
- 👥 **Multi-user Support** - Independent conversations for each user
- 📊 **Usage Monitoring** - Track engagement and chatbot statistics
- 🔒 **Secure** - User-authenticated sessions with privacy protection

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
    "messageCount": 1,
    "isActive": true
  }
}
```

**Note:** When you start a conversation, RaBot automatically sends you a friendly welcome message! You'll see it when you retrieve the conversation history.

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
    "botReply": "Great question about forums! 🎯 Forums are perfect for building community discussions. To create a forum post, you'll use the `/api/forums/post` endpoint with proper authentication. You'll need the `ForumPost` permission to do this.\n\nWant me to walk you through the specific steps, or do you have questions about permissions?",
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

### 1. Getting Started

**User:** "Hi! I'm new here."

**RaBot:** "Hey there! 👋 I'm RaBot, your friendly CMS assistant. I'm here to help you with anything related to the RaOS platform. What can I help you with today?"

### 2. Help with Content Management

**User:** "How do I publish a blog post?"

**RaBot:** "Blogs are awesome for sharing content! ✍️ To create a blog post, use the `/api/blogs/create` endpoint. You'll need the right permissions (usually `BlogCreate`), but I can help you check that.\n\nAre you looking to create your first blog post, or do you need help with something specific?"

### 3. Understanding Permissions

**User:** "I'm confused about permissions"

**RaBot:** "Ah, permissions! 🔐 The CMS uses Role-Based Access Control (RBAC) with different permission levels. Think of it like different keys for different doors - each role has access to specific features.\n\nIf you need to adjust your permissions, your administrator can help with that. Would you like to know more about what each role can do?"

### 4. Getting Help

**User:** "What can you help me with?"

**RaBot:** "I'd be happy to help! 😊 I can assist you with:\n\n📝 **Content Management** - Creating and editing content\n💬 **Forums & Blogs** - Setting up discussions and posts\n🔐 **Permissions** - Managing user access and roles\n⚙️ **Configuration** - System setup and settings\n\nWhat would you like to dive into?"

### 5. Troubleshooting

**User:** "I'm getting an error when I try to post"

**RaBot:** "Oh no! 😟 Sorry to hear you're running into issues. I'm here to help troubleshoot! Can you tell me a bit more about what's happening? The more details you share, the better I can assist you.\n\nFor example: What were you trying to do? What error message did you see?"

---

## Conversational Features

RaBot is designed to feel like chatting with a helpful friend:

### 🗣️ Natural Language
- **Talk naturally** - No need for formal commands or technical jargon
- **Ask in your own words** - "How do I..." or "Can you help me..." works great
- **Follow-up questions** - Have a back-and-forth conversation

### 😊 Friendly Personality
- **Warm greetings** - Every conversation starts with a friendly welcome
- **Encouraging tone** - Positive, supportive, and patient
- **Emojis** - Makes conversations feel more personal and engaging
- **Casual language** - Clear and approachable, not robotic

### 🎯 Helpful Guidance
- **Step-by-step help** - Breaking down complex tasks
- **Follow-up offers** - "Would you like to know more about...?"
- **Acknowledges your needs** - Understands what you're trying to accomplish
- **Proactive suggestions** - Offers related help and next steps

### 💡 Examples of Natural Conversation

**Instead of:** "execute forum creation API"
**Just say:** "How do I create a forum?"

**Instead of:** "display permission matrix"
**Just say:** "I'm confused about permissions"

**Instead of:** "enumerate API endpoints"
**Just say:** "What can I do with the API?"

---

## AI Integration

The chatbot integrates with the Language Model Processor when available:

### With AI Module
- **Context-aware responses** based on conversation history
- **Natural language understanding** - Comprehends intent and nuance
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
