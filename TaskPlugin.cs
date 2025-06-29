using Microsoft.SemanticKernel;
using System.ComponentModel;

public class TaskPlugin
{
    private readonly List<TaskItem> _tasks = new();
    private int _nextId = 1;

    [KernelFunction, Description("Yeni bir görev oluşturur")]
    public string CreateTask(
        [Description("Görev başlığı")] string title,
        [Description("Görev açıklaması")] string description,
        [Description("Öncelik seviyesi: Low, Medium, High")] string priority = "Medium")
    {
        var task = new TaskItem
        {
            Id = _nextId++,
            Title = title,
            Description = description,
            Priority = priority,
            CreatedAt = DateTime.Now,
            IsCompleted = false
        };

        _tasks.Add(task);
        return $"✅ Görev oluşturuldu: '{title}' (ID: {task.Id})";
    }

    [KernelFunction, Description("Tüm görevleri listeler")]
    public string ListTasks()
    {
        if (!_tasks.Any())
            return "📝 Henüz görev bulunmuyor.";

        var result = "📋 **Görev Listesi:**\n";
        foreach (var task in _tasks.OrderBy(t => t.CreatedAt))
        {
            var status = task.IsCompleted ? "✅" : "⏳";
            var priority = task.Priority switch
            {
                "High" => "🔴",
                "Medium" => "🟡",
                "Low" => "🟢",
                _ => "⚪"
            };

            result += $"{status} {priority} **{task.Title}** (ID: {task.Id})\n";
            result += $"   📝 {task.Description}\n";
            result += $"   📅 {task.CreatedAt:dd.MM.yyyy HH:mm}\n\n";
        }

        return result;
    }

    [KernelFunction, Description("Görevi tamamlanmış olarak işaretler")]
    public string CompleteTask([Description("Görev ID'si")] int taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            return $"❌ ID {taskId} ile görev bulunamadı.";

        task.IsCompleted = true;
        return $"✅ Görev tamamlandı: '{task.Title}'";
    }

    [KernelFunction, Description("Görevi siler")]
    public string DeleteTask([Description("Görev ID'si")] int taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            return $"❌ ID {taskId} ile görev bulunamadı.";

        _tasks.Remove(task);
        return $"🗑️ Görev silindi: '{task.Title}'";
    }

    [KernelFunction, Description("Görevler hakkında özet rapor verir")]
    public string GetTaskSummary()
    {
        var total = _tasks.Count;
        var completed = _tasks.Count(t => t.IsCompleted);
        var pending = total - completed;
        var highPriority = _tasks.Count(t => t.Priority == "High" && !t.IsCompleted);

        return $"📊 **Görev Özeti:**\n" +
               $"• Toplam Görev: {total}\n" +
               $"• Tamamlanan: {completed}\n" +
               $"• Bekleyen: {pending}\n" +
               $"• Yüksek Öncelikli: {highPriority}";
    }
}