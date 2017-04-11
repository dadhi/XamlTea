using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;

namespace Tea.Wpf
{
    public static class Wpf
    {
        public static INativeUI CreateUI(ContentControl root)
        {
            return new WpfUI(Update, root);
        }

        private class WpfUI : INativeUI
        {
            private readonly Func<UIUpdate, ContentControl, unit> _apply;
            private readonly ContentControl _root;

            public WpfUI(Func<UIUpdate, ContentControl, unit> apply, ContentControl root)
            {
                _apply = apply;
                _root = root;
            }

            public void Send(ImList<UIUpdate> uiUpdates)
            {
                uiUpdates.To(unit._, (update, _) => _apply(update, _root));
            }
        }

        private static UIElement CreateUI(UI ui)
        {
            if (ui == null)
                throw new ArgumentNullException(nameof(ui));

            if (ui is UI.Text)
                return new Label { Content = ui.Content };

            var input = ui as UI.Input;
            if (input != null)
            {
                var elem = new TextBox { Text = input.Content };
                var ev = input.Changed.Value;
                elem.TextChanged += (sender, _) => ev.Value(((TextBox)sender).Text);
                return elem;
            }

            var button = ui as UI.Button;
            if (button != null)
            {
                var elem = new Button
                {
                    Content = button.Content,
                    IsEnabled = ui.Props.Get(Prop.IsEnabled.Enabled).Value
                };
                var ev = button.Clicked.Value;
                elem.Click += (sender, _) => ev.Value(unit._);
                return elem;
            }

            var div = ui as UI.Panel;
            if (div != null)
            {
                var parts = div.Parts.Map(CreateUI);
                var orientation = div.Layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                var elem = new StackPanel { Orientation = orientation };
                parts.To(0, (p, _) => elem.Children.Add(p));
                return elem;
            }

            var check = ui as UI.CheckBox;
            if (check != null)
            {
                var elem = new CheckBox { Content = check.Content, IsChecked = check.IsChecked };
                var ev = check.Changed.Value;
                elem.Checked += (sender, _) => ev.Value(true);
                elem.Unchecked += (sender, _) => ev.Value(false);
                return elem;
            }

            throw new NotSupportedException("The type of UI is not supported: " + ui.GetType());
        }

        private static unit UpdateUI(UI ui, UIElement elem)
        {
            if (ui is UI.Text)
                ((Label)elem).Content = ui.Content;

            else if (ui is UI.Input)
                ((TextBox)elem).Text = ui.Content;

            else if (ui is UI.Button)
            {
                var button = (Button)elem;
                button.Content = ui.Content;
                button.IsEnabled = ui.Props.Get(Prop.IsEnabled.Enabled).Value;
            }

            return unit._;
        }

        private static Panel LocatePanel(ImList<int> path, ContentControl root)
        {
            if (path.IsEmpty)
                return root.Content as Panel;
            var panel = LocatePanel(path.Tail, root);
            return (Panel)panel.Children[path.Head];
        }

        private static unit Update(UIUpdate update, ContentControl root)
        {
            var insertUI = update as UIUpdate.Insert;
            if (insertUI != null)
            {
                var path = insertUI.Path;
                if (path.IsEmpty)
                    root.Content = CreateUI(insertUI.UI);
                else
                    LocatePanel(path.Tail, root).Children.Insert(path.Head, CreateUI(insertUI.UI));
                return unit._;
            }

            var updateUI = update as UIUpdate.Update;
            if (updateUI != null)
            {
                var path = updateUI.Path;
                var elem = path.IsEmpty
                    ? (UIElement)root.Content
                    : LocatePanel(path.Tail, root).Children[path.Head];
                return UpdateUI(updateUI.UI, elem);
            }

            var replaceUI = update as UIUpdate.Replace;
            if (replaceUI != null)
            {
                var path = replaceUI.Path;
                if (path.IsEmpty)
                    root.Content = CreateUI(replaceUI.UI);
                else
                {
                    var children = LocatePanel(path.Tail, root).Children;
                    children.RemoveAt(path.Head);
                    children.Insert(path.Head, CreateUI(replaceUI.UI));
                }
                return unit._;
            }

            var removeUI = update as UIUpdate.Remove;
            if (removeUI != null)
            {
                var path = removeUI.Path;
                if (!path.IsEmpty)
                    LocatePanel(path.Tail, root).Children.RemoveAt(path.Head);
            }

            // Skip event
            //| EventUI _-> ()
            return unit._;
        }
    }
}
