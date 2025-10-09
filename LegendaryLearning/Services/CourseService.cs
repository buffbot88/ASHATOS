using System.Collections.Concurrent;
using LegendaryLearning.Abstractions;
using Abstractions;

namespace LegendaryLearning.Services;

/// <summary>
/// Service for course management and retrieval.
/// </summary>
public class CourseService : ICourseService
{
    private readonly ConcurrentDictionary<string, Course> _courses = new();

    public async Task<List<Course>> GetCoursesAsync(string permissionLevel)
    {
        await Task.CompletedTask;
        
        return _courses.Values
            .Where(c => c.IsActive && c.PermissionLevel == permissionLevel)
            .OrderBy(c => c.Title)
            .ToList();
    }

    public async Task<Course?> GetCourseByIdAsync(string courseId)
    {
        await Task.CompletedTask;
        
        return _courses.TryGetValue(courseId, out var course) ? course : null;
    }

    public async Task<(bool success, string message)> UpdateCourseAsync(Course course)
    {
        await Task.CompletedTask;
        
        _courses[course.Id] = course;
        course.UpdatedAt = DateTime.UtcNow;
        
        Console.WriteLine($"[CourseService] Updated course: {course.Title}");
        return (true, "Course updated successfully");
    }

    public void AddCourse(Course course)
    {
        _courses[course.Id] = course;
    }

    public int GetCourseCount() => _courses.Count;
}
