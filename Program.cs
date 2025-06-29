using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🤖 AI-Powered Task Manager'a Hoş Geldiniz!");
        Console.WriteLine("Semantic Kernel ile çalışan akıllı görev yöneticisi\n");

        // API Key kontrolü
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ OPENAI_API_KEY environment variable gerekli!");
            Console.WriteLine("Örnek komutları görmek için DEMO modunda çalışıyor...\n");
            await RunDemoMode();
            return;
        }

        await RunWithAI(apiKey);
    }

    private static async Task RunDemoMode()
    {
        var plugin = new TaskPlugin();

        Console.WriteLine("📝 Demo Görevler Oluşturuluyor...\n");

        // Demo görevler
        Console.WriteLine(plugin.CreateTask("Website Tasarımı", "Yeni portfolyo sitesi tasarla", "High"));
        Console.WriteLine(plugin.CreateTask("API Geliştirme", "REST API endpointlerini kodla", "Medium"));
        Console.WriteLine(plugin.CreateTask("Test Yazma", "Unit testleri yaz", "Low"));

        Console.WriteLine("\n" + plugin.ListTasks());
        Console.WriteLine(plugin.CompleteTask(1));
        Console.WriteLine("\n" + plugin.GetTaskSummary());

        Console.WriteLine("\n🎯 Gerçek AI deneyimi için OpenAI API key'i ekleyin!");
    }

    private static async Task RunWithAI(string apiKey)
    {
        // Semantic Kernel kurulumu
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey);

        var kernel = builder.Build();

        // Plugin'i kernel'e ekle
        var taskPlugin = new TaskPlugin();
        kernel.Plugins.AddFromObject(taskPlugin, "TaskManager");

        // Chat completion servisi
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        Console.WriteLine("💡 İpucu: 'çıkış' yazarak programdan çıkabilirsiniz.");
        Console.WriteLine("Örnek: 'Website projesi için yüksek öncelikli bir görev oluştur'\n");

        // Ana döngü
        while (true)
        {
            Console.Write("🗣️  Siz: ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput) ||
                userInput.ToLower().Contains("çıkış") ||
                userInput.ToLower().Contains("exit"))
            {
                Console.WriteLine("👋 Görüşmek üzere!");
                break;
            }

            try
            {
                // System prompt - AI'ya nasıl davranacağını söyle
                var systemPrompt = @"   Sen bir görev yönetim asistanısın. Kullanıcının isteklerini anlayıp uygun fonksiyonları çağır.
                                            Kullanılabilir fonksiyonlar:
                                            - CreateTask: Yeni görev oluştur
                                            - ListTasks: Görevleri listele  
                                            - CompleteTask: Görev tamamla
                                            - DeleteTask: Görev sil
                                            - GetTaskSummary: Özet rapor

                                            Türkçe ve dostça yanıtlar ver. Emoji kullan.";
                // Chat history
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(userInput);

                // OpenAI prompt execution settings
                var executionSettings = new OpenAIPromptExecutionSettings()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                // AI yanıtı al
                var response = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    kernel);

                Console.WriteLine($"🤖 AI: {response.Content}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}\n");
            }
        }
    }
}