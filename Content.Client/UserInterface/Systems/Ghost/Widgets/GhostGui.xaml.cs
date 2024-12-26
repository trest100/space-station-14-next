using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.Ghost.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Content.Client._CorvaxNext.UserInterface.Systems.Ghost.Controls;

namespace Content.Client.UserInterface.Systems.Ghost.Widgets;

[GenerateTypedNameReferences]
public sealed partial class GhostGui : UIWidget
{
    public GhostTargetWindow TargetWindow { get; }
	public GhostBarRulesWindow GhostBarWindow { get; } // Corvax-Next-GhostBar

    public event Action? RequestWarpsPressed;
    public event Action? ReturnToBodyPressed;
    public event Action? GhostRolesPressed;
	public event Action? GhostBarPressed; // Corvax-Next-GhostBar

    public GhostGui()
    {
        RobustXamlLoader.Load(this);

        TargetWindow = new GhostTargetWindow();

		GhostBarWindow = new GhostBarRulesWindow(); // Corvax-Next-GhostBar

        MouseFilter = MouseFilterMode.Ignore;

        GhostWarpButton.OnPressed += _ => RequestWarpsPressed?.Invoke();
        ReturnToBodyButton.OnPressed += _ => ReturnToBodyPressed?.Invoke();
        GhostRolesButton.OnPressed += _ => GhostRolesPressed?.Invoke();
		GhostBarButton.OnPressed += _ => GhostBarPressed?.Invoke(); // Corvax-Next-GhostBar
    }

    public void Hide()
    {
        TargetWindow.Close();
		GhostBarWindow.Close(); // Corvax-Next-GhostBar
        Visible = false;
    }

    public void Update(int? roles, bool? canReturnToBody)
    {
        ReturnToBodyButton.Disabled = !canReturnToBody ?? true;

        if (roles != null)
        {
            GhostRolesButton.Text = Loc.GetString("ghost-gui-ghost-roles-button", ("count", roles));
            if (roles > 0)
            {
                GhostRolesButton.StyleClasses.Add(StyleBase.ButtonCaution);
            }
            else
            {
                GhostRolesButton.StyleClasses.Remove(StyleBase.ButtonCaution);
            }
        }

        TargetWindow.Populate();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            TargetWindow.Dispose();
			GhostBarWindow.Dispose(); // Corvax-Next-GhostBar
        }
    }
}
