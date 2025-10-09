# ASHAT Personality & Emotion - Quick Start Guide

Get started with ASHAT's emotional intelligence features in minutes!

## 🚀 Quick Start

### Step 1: Choose a Personality

Configure ASHAT's personality to match your preferences:

```bash
# Use the friendly assistant (default)
ashatpersonality set myuser friendly

# Or choose a different personality
ashatpersonality set myuser professional  # For serious work
ashatpersonality set myuser playful       # For creative sessions
ashatpersonality set myuser calm          # For stressful situations
ashatpersonality set myuser coach         # For goal achievement
ashatpersonality set myuser wise          # For decision making
```

### Step 2: Let ASHAT Understand Your Emotions

ASHAT automatically detects emotions in your messages:

```bash
# When you're excited
ashatemot detect myuser "Wow! This is amazing!"
# ASHAT detects: Excited (85% confidence)

# When you're frustrated
ashatemot detect myuser "I'm stuck on this problem"
# ASHAT detects: Supportive response needed

# When you're grateful
ashatemot detect myuser "Thank you so much!"
# ASHAT detects: Grateful (90% confidence)
```

### Step 3: Build a Relationship

Interact with ASHAT to build trust and rapport:

```bash
# Record your interactions
ashatrel interact myuser positive

# Let ASHAT learn your preferences
ashatrel prefer myuser likes_emojis=true
ashatrel prefer myuser prefers_concise=false

# View your relationship status
ashatrel status myuser
```

## 📋 Common Use Cases

### Use Case 1: I want ASHAT to be more formal

```bash
# Set professional personality
ashatpersonality set myuser professional

# Set preference for formal communication
ashatrel prefer myuser prefers_formal=true
```

### Use Case 2: I want encouragement when working

```bash
# Set enthusiastic coach personality
ashatpersonality set myuser coach

# ASHAT will provide motivation automatically
# You can also request reinforcement:
ashatrel reinforce myuser "completed a difficult task"
# Response: "Amazing work on completed a difficult task! You're making excellent progress! 🌟"
```

### Use Case 3: I want ASHAT to be calm and patient

```bash
# Set calm guide personality
ashatpersonality set myuser calm

# ASHAT will respond with patience and reassurance
ashatemot respond myuser frustrated
# Response: "I can sense your frustration. Take a breath. We'll get through this together. 💙"
```

### Use Case 4: Track my progress with ASHAT

```bash
# View relationship milestones
ashatrel milestone myuser

# Check psychological context
ashatrel psychology myuser

# See interaction history
ashatrel status myuser
```

## 🎭 Personality Comparison

| Personality | Best For | Communication Style | Emoji Use |
|------------|----------|---------------------|-----------|
| **Friendly Assistant** | Daily work, general tasks | Warm, encouraging | Yes ✓ |
| **Professional Mentor** | Learning, serious work | Clear, focused | No ✗ |
| **Playful Companion** | Creative work, brainstorming | Fun, enthusiastic | Yes ✓ |
| **Calm Guide** | Stressful situations | Patient, soothing | Minimal |
| **Enthusiastic Coach** | Goal achievement, motivation | Energetic, positive | Yes ✓ |
| **Wise Advisor** | Decision making, planning | Thoughtful, measured | No ✗ |

## 💡 Pro Tips

### Tip 1: Match Personality to Task

```bash
# Use different personalities for different contexts
ashatpersonality set alice professional  # For code reviews
ashatpersonality set bob playful        # For hackathons
ashatpersonality set charlie calm       # For debugging sessions
```

### Tip 2: Let ASHAT Learn Your Preferences

```bash
# Record multiple preferences
ashatrel prefer myuser likes_humor=true
ashatrel prefer myuser detail_level=high
ashatrel prefer myuser response_length=medium
ashatrel prefer myuser prefers_examples=true

# ASHAT will adapt to your preferences over time
```

### Tip 3: Celebrate Your Wins

```bash
# Record achievements for positive reinforcement
ashatrel reinforce myuser "fixed a critical bug"
ashatrel reinforce myuser "learned a new technology"
ashatrel reinforce myuser "helped a teammate"

# ASHAT will celebrate with you!
```

### Tip 4: Monitor Your Relationship Growth

```bash
# Check your relationship level regularly
ashatrel status myuser

# Relationship levels:
# 🆕 New (0-4 interactions)
# 👋 Acquainted (5-19 interactions)
# 🤝 Familiar (20-49 interactions)
# 💙 Trusted (50-99 interactions)
# 💖 Bonded (100+ interactions)
```

## 🔧 Advanced Configuration

### Custom Personality via API

```csharp
var personalityModule = new AshatPersonalityModule();
personalityModule.Initialize(null);

var customPersonality = new AshatPersonality
{
    Name = "My Custom Personality",
    Openness = 0.9f,          // Very creative
    Conscientiousness = 0.7f,  // Organized
    Extraversion = 0.6f,       // Moderately outgoing
    Agreeableness = 0.9f,      // Very friendly
    EmotionalStability = 0.8f, // Calm
    Formality = 0.4f,          // Casual
    Enthusiasm = 0.8f,         // Enthusiastic
    UseEmojis = true,
    ExpressEmpathy = true
};

var config = new PersonalityConfiguration
{
    UserId = "myuser",
    Template = PersonalityTemplate.Custom,
    CustomPersonality = customPersonality,
    EnableEmotionalExpressions = true,
    EnableRelationshipBuilding = true,
    MaintainProfessionalBoundaries = true
};

await personalityModule.SetPersonalityConfigAsync("myuser", config);
```

### Emotion Detection in Code

```csharp
var emotionModule = new AshatEmotionModule();
emotionModule.Initialize(null);

// Detect emotion from user message
var emotion = await emotionModule.DetectUserEmotionAsync(
    "myuser", 
    "I'm really excited about this new feature!"
);

Console.WriteLine($"Detected: {emotion.DetectedEmotion}");
Console.WriteLine($"Confidence: {emotion.Confidence:P0}");

// Generate appropriate response
var response = await emotionModule.GenerateResponseAsync(
    "myuser",
    emotion.DetectedEmotion
);

Console.WriteLine(response.ResponseText);
```

### Relationship Tracking in Code

```csharp
var relationshipModule = new AshatRelationshipModule();
relationshipModule.Initialize(null);

// Record a positive interaction
await relationshipModule.RecordInteractionAsync("myuser", positive: true);

// Learn a preference
await relationshipModule.LearnPreferenceAsync(
    "myuser",
    "favorite_language",
    "C#"
);

// Get relationship status
var relationship = await relationshipModule.GetRelationshipAsync("myuser");

Console.WriteLine($"Level: {relationship.Level}");
Console.WriteLine($"Trust: {relationship.TrustScore:P0}");
Console.WriteLine($"Interactions: {relationship.InteractionCount}");
```

## 📚 Learn More

- **Full Documentation**: See `ASHAT_PERSONALITY_EMOTION_IMPLEMENTATION.md`
- **README**: Check `RaCore/Modules/Extensions/Ashat/README.md`
- **Tests**: Review `RaCore/Tests/AshatPersonalityEmotionTests.cs` for examples
- **Models**: Explore `Abstractions/AshatPersonalityModels.cs` for data structures

## ❓ FAQ

**Q: Can I change personality during a session?**  
A: Yes! You can change personality at any time using `ashatpersonality set <userId> <template>`

**Q: Will ASHAT remember my preferences?**  
A: Yes, preferences are stored and persist across sessions. ASHAT learns more about you over time.

**Q: How does emotion detection work?**  
A: ASHAT analyzes keywords, punctuation, and context in your messages to detect emotional states.

**Q: Can I turn off emoji use?**  
A: Yes, choose a personality like "Professional Mentor" or "Wise Advisor" that doesn't use emojis.

**Q: How do relationship levels work?**  
A: Levels progress based on interaction count and trust score. More positive interactions = stronger relationship.

**Q: Is my data private?**  
A: Yes, all personality, emotion, and relationship data is stored locally and user-specific.

## 🎉 Get Started Now!

```bash
# Step 1: Set your personality
ashatpersonality set myuser friendly

# Step 2: Start interacting
ashatrel interact myuser positive

# Step 3: Let ASHAT know your preferences
ashatrel prefer myuser likes_encouragement=true

# You're all set! Enjoy your emotionally intelligent ASHAT experience! 🌟
```

---

**Need help?** Check the full documentation or ask ASHAT! 😊
