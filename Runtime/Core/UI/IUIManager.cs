namespace Mosambi.UI
{
    public interface IUIManager
    {
        T GetView<T>() where T : UIView;
        void ShowScreen<T>() where T : UIView;
        void ShowPopup<T>() where T : UIView;
        void GoBack();
    }
}