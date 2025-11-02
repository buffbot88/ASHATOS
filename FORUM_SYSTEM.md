# Forum System - Professional vBulletin v4-Style Implementation

## Overview

The ASHAT Os forum system provides a professional, feature-rich discussion platform built entirely with Razor Pages and .NET 9.0. The implementation is inspired by vBulletin v4, offering a familiar and powerful forum experience with modern security features.

## Features

### User-Facing Features

#### Forum Index (`/cms/forums`)
- **Category Organization**: 4 major categories (General Discussion, Technical Support, Development, Community)
- **11 Active Forums**: Covering announcements, chat, support, development, and community topics
- **Real-time Statistics**: Total threads, posts, members, and online users
- **Last Activity Tracking**: Shows most recent post author and timestamp for each forum
- **Visual Icons**: Emoji-based category and forum icons for easy navigation
- **Responsive Layout**: Mobile-optimized grid design

#### Forum View (`/cms/forums/view?id=X`)
- **Thread Listing**: Clean, organized display of all threads in a forum
- **Sticky Threads**: Pinned threads always appear at top with special styling
- **Thread Prefixes**: Support for Announcement, Question, Solved, and custom prefixes
- **Thread Status Indicators**: 
  - 🔒 Locked threads (read-only)
  - 📌 Pinned/Sticky threads
  - 💬 Regular threads
- **Metadata Display**: Reply count, view count, author, and last post information
- **Pagination**: 20 threads per page with page navigation
- **Quick Actions**: New thread button prominently displayed

#### Thread View (`/cms/forums/thread?id=X`)
- **Rich User Profiles**: Each post includes:
  - Avatar (initial-based with custom styling)
  - Username and user title (Member, Administrator, etc.)
  - Post count and join date
  - Location (optional)
  - User signature (optional, multi-line support)
- **Post Display**: Clean, readable layout with proper spacing
- **Post Actions**: Quote, Like, Report, Edit buttons
- **Quick Reply**: Inline form for fast responses
- **Pagination**: 10 posts per page
- **Thread Information**: Title, author, date, views, and reply count
- **Breadcrumb Navigation**: Easy navigation back to forum index

#### Moderation Panel (`/cms/forums/moderate`)
- **Statistics Dashboard**: 
  - Total threads and posts
  - Deleted posts count
  - Locked threads count
  - Banned users count
  - Active warnings count
- **Flagged Posts Queue**: Review and action flagged content
- **Locked Threads Management**: View and unlock threads
- **Banned Users List**: Review bans and unban users
- **Deleted Posts Archive**: View and restore deleted content
- **Quick Actions**: Approve, delete, warn, view buttons for each item

### Technical Features

#### Security
- **XSS Protection**: All user content properly HTML-encoded
- **Content Moderation Integration**: Automatic scanning via ContentModerationModule
- **Parental Controls**: Age-appropriate content filtering
- **User Banning**: System-wide forum bans
- **Warning System**: Three-strike auto-ban mechanism
- **Post Deletion Tracking**: Audit trail for moderation actions

#### Architecture
- **Razor Pages**: Pure .NET implementation, no PHP dependencies
- **Modular Design**: Clean separation between UI and business logic
- **API-Ready**: Models designed for easy REST API integration
- **Responsive CSS**: Mobile-first design with CSS Grid
- **Theme Consistency**: Purple gradient theme matching site design

#### ForumModule Enhancements
- **Interface Extensions**:
  - `PinThreadAsync()` - Pin/unpin threads
  - Enhanced `ForumPost` model with `IsPinned` and `Prefix` properties
- **Console Commands**:
  - `forum pin <postId>` - Pin a thread
  - `forum unpin <postId>` - Unpin a thread
  - Existing commands: stats, posts, delete, lock, unlock, warn, ban, unban

## Theme & Design

### Visual Design
- **Color Scheme**: Purple gradient (matching main site)
  - Primary: `#8b2fc7` to `#6a1b9a`
  - Background: Dark gradient from black to purple
  - Text: Light purple/white tones
- **Typography**: Segoe UI font family
- **Icons**: Emoji-based for universal compatibility
- **Shadows**: Subtle glowing effects on hover
- **Borders**: Purple-tinted rgba borders

### Layout
- **Grid-Based**: CSS Grid for flexible, responsive layouts
- **Card Style**: Each major section is a rounded card with borders
- **Hover Effects**: Interactive elements have smooth transitions
- **Mobile Responsive**: Stacks vertically on small screens

## Integration Points

### Current Integration
- **ForumModule**: Backend logic for forum operations
- **ContentModerationModule**: Automatic content scanning
- **ParentalControlModule**: Age-appropriate filtering

### Future Integration Opportunities
- **Authentication**: User login and permissions
- **Database**: SQLite/SQL Server for persistence
- **Email**: Notifications and subscriptions
- **Search**: Full-text search across forums
- **File Upload**: Attachments support
- **Rich Text Editor**: BBCode or Markdown support

## Usage Examples

### Creating a Forum
Forums are currently defined in `IndexModel.LoadForumCategories()`. To add a new forum:

```csharp
new ForumInfo
{
    Id = 12,
    Title = "Your Forum Title",
    Description = "Your forum description",
    Icon = "🎯",
    TopicCount = 0,
    PostCount = 0,
    LastPostUser = "",
    LastPostDate = DateTime.UtcNow
}
```

### Moderating Content
Use console commands through the ForumModule:

```bash
forum stats              # View forum statistics
forum pin post_1         # Pin a thread
forum lock post_2        # Lock a thread
forum warn user_1 spam   # Warn a user
forum ban user_2 abuse   # Ban a user
```

### Accessing Moderation Panel
Navigate to `/cms/forums/moderate` to access the full moderation interface.

## File Structure

```
ASHATCore/Pages/CMS/Forums/
├── Index.cshtml              # Forum index page
├── Index.cshtml.cs          # Forum index model
├── View.cshtml              # Forum threads list
├── View.cshtml.cs           # Forum threads model
├── Thread.cshtml            # Individual thread view
├── Thread.cshtml.cs         # Thread view model
├── Moderate.cshtml          # Moderation panel
└── Moderate.cshtml.cs       # Moderation model

ASHATCore/Modules/Extensions/Forum/
└── ForumModule.cs           # Backend forum logic

Abstractions/
└── IForumModule.cs          # Forum interface and models
```

## Customization

### Adding Thread Prefixes
Update the thread prefix CSS in the view files:

```css
.prefix-yourtype {
    background: linear-gradient(135deg, #color1 0%, #color2 100%);
    color: white;
}
```

### Changing Theme Colors
All CSS uses CSS variables pattern. To change colors, update the gradient and color values in the `<style>` sections of each page.

### Adding User Roles
Extend the `UserTitle` property in `PostInfo` and update the display logic in `Thread.cshtml`.

## Performance Considerations

- **Pagination**: Default 20 threads per page, 10 posts per page
- **In-Memory Storage**: Currently uses `ConcurrentDictionary` (replace with database for production)
- **Caching**: Consider adding output caching for read-heavy operations
- **Lazy Loading**: Posts and threads loaded on-demand

## Browser Support

- **Modern Browsers**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Mobile**: iOS Safari, Chrome Mobile, Samsung Internet
- **Features Used**: CSS Grid, Flexbox, CSS custom properties, ES6+

## Accessibility

- **Semantic HTML**: Proper use of headings, lists, and sections
- **Keyboard Navigation**: All interactive elements accessible via keyboard
- **Screen Readers**: ARIA labels where appropriate
- **Color Contrast**: WCAG AA compliant color combinations
- **Responsive Text**: Readable on all screen sizes

## Future Enhancements

### Planned Features
- [ ] New thread creation form
- [ ] Reply submission handling
- [ ] Post editing functionality
- [ ] Thread search implementation
- [ ] User reputation system
- [ ] Thread subscriptions
- [ ] Email notifications
- [ ] Rich text editor (BBCode/Markdown)
- [ ] File attachments
- [ ] Private messaging integration
- [ ] Multi-quote functionality
- [ ] Thread tagging
- [ ] Advanced search filters
- [ ] User ignore list
- [ ] Thread polls
- [ ] Social media sharing

### Technical Improvements
- [ ] Database persistence layer
- [ ] REST API endpoints
- [ ] WebSocket real-time updates
- [ ] Full-text search indexing
- [ ] Image optimization and CDN
- [ ] Caching strategy
- [ ] Rate limiting
- [ ] Spam detection
- [ ] Analytics integration

## Support

For questions or issues with the forum system:
1. Check the ModeratePanel for statistics and health
2. Use `forum help` console command for available operations
3. Review `ForumModule.cs` for backend logic
4. Refer to this documentation for feature explanations

## Credits

- **Design Inspiration**: vBulletin v4
- **Framework**: ASP.NET Core 9.0 with Razor Pages
- **Theme**: AGP Studios Purple Gradient
- **Icons**: Unicode Emoji
- **Architecture**: ASHAT Os Modular System
