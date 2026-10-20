
namespace InventorySystem.Forms
{
    partial class PartsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Main logic is in PartsForm.cs manual InitializeComponent, 
        // but Visual Studio expects this file to exist for the Designer.
        // We can leave it empty if InitializeComponent is in .cs, 
        // OR move InitializeComponent here. 
        // For simplicity, I'll put a dummy InitializeComponent here to satisfy the file requirement
        // but the actual one is in PartsForm.cs as written above.
        // Wait, C# partial classes merge. If I defined InitializeComponent in PartsForm.cs, I shouldn't define it here.
        // But usually it lives here.
        // I will leave this file mostly empty to avoid conflicts, just class definition.
    }
}
