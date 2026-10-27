
using PovraRegistra.Models;
using PovraRegistra.ViewModels;

namespace PovraRegistra.Views;

public partial class SkinFormView : ContentPage
{
    public SkinFormView(ItemSkin? skin = null)
    {
        InitializeComponent();

        BindingContext = new SkinFormViewModel(skin);
    }
}
