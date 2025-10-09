using System.Collections.Concurrent;
using LegendaryLearning.Abstractions;
using Abstractions;

namespace LegendaryLearning.Services;

/// <summary>
/// Service for lesson management and retrieval.
/// </summary>
public class LessonService : ILessonService
{
    private readonly ConcurrentDictionary<string, Lesson> _lessons = new();
    private readonly ConcurrentDictionary<string, List<string>> _courseLessons = new();

    public async Task<List<Lesson>> GetLessonsAsync(string courseId)
    {
        await Task.CompletedTask;
        
        if (!_courseLessons.TryGetValue(courseId, out var lessonIds))
        {
            return new List<Lesson>();
        }
        
        return lessonIds
            .Select(id => _lessons.TryGetValue(id, out var lesson) ? lesson : null)
            .Where(l => l != null)
            .Cast<Lesson>()
            .OrderBy(l => l.OrderIndex)
            .ToList();
    }

    public async Task<Lesson?> GetLessonByIdAsync(string lessonId)
    {
        await Task.CompletedTask;
        
        return _lessons.TryGetValue(lessonId, out var lesson) ? lesson : null;
    }

    public async Task<(bool success, string message)> UpdateLessonAsync(Lesson lesson)
    {
        await Task.CompletedTask;
        
        _lessons[lesson.Id] = lesson;
        lesson.UpdatedAt = DateTime.UtcNow;
        
        if (!_courseLessons.ContainsKey(lesson.CourseId))
        {
            _courseLessons[lesson.CourseId] = new List<string>();
        }
        
        if (!_courseLessons[lesson.CourseId].Contains(lesson.Id))
        {
            _courseLessons[lesson.CourseId].Add(lesson.Id);
        }
        
        Console.WriteLine($"[LessonService] Updated lesson: {lesson.Title}");
        return (true, "Lesson updated successfully");
    }

    public void AddLesson(Lesson lesson)
    {
        _lessons[lesson.Id] = lesson;
        
        if (!_courseLessons.ContainsKey(lesson.CourseId))
        {
            _courseLessons[lesson.CourseId] = new List<string>();
        }
        
        _courseLessons[lesson.CourseId].Add(lesson.Id);
    }

    public int GetLessonCount() => _lessons.Count;
    
    public int GetTotalLessonCount(string courseId)
    {
        return _courseLessons.TryGetValue(courseId, out var lessons) ? lessons.Count : 0;
    }
}
