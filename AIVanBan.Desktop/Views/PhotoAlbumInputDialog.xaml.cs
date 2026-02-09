using System.Windows;

namespace AIVanBan.Desktop.Views;

public partial class PhotoAlbumInputDialog : Window
{
    public string AlbumName { get; private set; } = "";
    public string AlbumDescription { get; private set; } = "";

    public PhotoAlbumInputDialog(string title, string defaultName = "", string defaultDescription = "")
    {
        InitializeComponent();
        
        Title = title;
        txtName.Text = defaultName;
        txtDescription.Text = defaultDescription;
        
        // Force enable IME for Vietnamese input
        System.Windows.Input.InputMethod.SetPreferredImeState(txtName, System.Windows.Input.InputMethodState.On);
        System.Windows.Input.InputMethod.SetPreferredImeState(txtDescription, System.Windows.Input.InputMethodState.On);
        
        Loaded += (s, e) => 
        {
            txtName.Focus();
            txtName.SelectAll();
        };
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        AlbumName = txtName.Text;
        AlbumDescription = txtDescription.Text;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
