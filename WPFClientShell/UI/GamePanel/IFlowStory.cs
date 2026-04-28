using System.Windows.Documents;

namespace WPFClientShell.UI.GamePanel
{
    public interface IFlowStory
    {
        void StoryNewLine();
        void StoryRun(Run line);
        void StoryClear();
    }
}