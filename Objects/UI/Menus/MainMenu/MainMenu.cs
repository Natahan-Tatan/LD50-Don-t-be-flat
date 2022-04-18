using Godot;
using System;

namespace UI
{
    public class MainMenu : Control
    {
        [Export(PropertyHint.File,"*.tscn")]
        public String Game {get; set;}
        public void _on_PlayButton_pressed()
        {
            GetTree().ChangeScene(Game);
        }

        public void _on_AboutButton_pressed()
        {
            GetNode<PopupDialog>("AboutWindow").Show();
        }

        public void _on_CloseButton_pressed()
        {
            GetNode<PopupDialog>("AboutWindow").Hide();
        }

        public void _on_RichTextLabel_meta_clicked(String meta)
        {
            OS.ShellOpen(meta);
        }
    }

}