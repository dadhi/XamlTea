using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;

namespace Tea.Wpf
{
    public class WpfUI : INativeUI
    {
        /// <summary>Initializes the UI bound to provided content control.</summary>
        public static INativeUI Init(ContentControl root) => new WpfUI(root);

        private readonly ContentControl _rootControl;
        private WpfUI(ContentControl rootControl) => _rootControl = rootControl;

        /// <summary>Applies the updates.</summary>
        public void ApplyDiffs(ImList<UIDiff> diffs) =>
            diffs.Do(update => _rootControl.Dispatcher.Invoke(() => Apply(update, _rootControl)));

        private static void Apply(UIDiff uiDiff, ContentControl root)
        {
            var path = uiDiff.Path;
            switch (uiDiff)
            {
                case UIDiff.Insert insert:
                    if (path.IsEmpty)
                        root.Content = CreateUI(insert.UI);
                    else
                        Locate(path.Tail, root).Children.Insert(path.Head, CreateUI(insert.UI));
                    break;

                case UIDiff.Update update:
                    var elem = path.IsEmpty
                        ? (UIElement)root.Content
                        : Locate(path.Tail, root).Children[path.Head];

                    Update(update.UI, elem);
                    break;

                case UIDiff.Replace replace:
                    if (path.IsEmpty)
                        root.Content = CreateUI(replace.UI);
                    else
                    {
                        var children = Locate(path.Tail, root).Children;
                        children.RemoveAt(path.Head);
                        children.Insert(path.Head, CreateUI(replace.UI));
                    }
                    break;

                case UIDiff.Remove _:
                    if (!path.IsEmpty)
                        Locate(path.Tail, root).Children.RemoveAt(path.Head);
                    break;

                case UIDiff.Event _:
                    // do nothing for events because they are raised before applying update in main MVU loop
                    break;
            }
        }

        private static Panel Locate(ImList<int> path, ContentControl contentControl) =>
            path.IsEmpty
                ? (Panel)contentControl.Content
                : (Panel)Locate(path.Tail, contentControl).Children[path.Head];

        private static UIElement CreateUI(UI ui)
        {
            if (ui == null)
                throw new ArgumentNullException(nameof(ui));

            if (ui is UI.Text)
                return new Label { Content = ui.Content };

            if (ui is UI.Input input)
            {
                var elem = new TextBox { Text = input.Content };
                var ev = input.Changed.Value;
                elem.TextChanged += (sender, _) => ev.Value(((TextBox)sender).Text);
                return elem;
            }

            if (ui is UI.Button button)
            {
                var elem = new Button { Content = button.Content };

                var ev = button.Clicked.Value;
                elem.Click += (sender, _) => ev.Value(Unit.unit);

                return elem;
            }

            if (ui is UI.Panel panel)
            {
                var orientation = panel.Layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                var elem = new StackPanel { Orientation = orientation };

                var children = panel.Parts.Map(CreateUI);
                children.Do(x => elem.Children.Add(x));

                return elem;
            }

            if (ui is UI.CheckBox check)
            {
                var elem = new CheckBox
                {
                    Content = check.Content,
                    IsChecked = check.IsChecked,
                };

                var ev = check.Changed.Value;
                elem.Checked += (sender, _) => ev.Value(true);
                elem.Unchecked += (sender, _) => ev.Value(false);

                return elem;
            }

            throw new NotSupportedException("The type of UI is not supported: " + ui.GetType());
        }

        //private static readonly Thickness DefaultMargin = new Thickness(2);
        //private static FrameworkElement WithStyles(FrameworkElement elem, ImList<Style> styles)
        //{
        //    elem.Margin = DefaultMargin;
        //    styles.Do(style =>
        //    {
        //        if (style is Style.Width width)
        //            elem.Width = width.Value;
        //        else if (style is Style.Height height)
        //            elem.Height = height.Value;
        //        else if (style is Style.IsEnabled isEnabled)
        //            elem.IsEnabled = isEnabled.Value;
        //        else if (style is Style.Tooltip tooltip)
        //            elem.ToolTip = tooltip.Value;
        //    });
        //    return elem;
        //}

        private static void Update(UI ui, UIElement elem)
        {
            switch (ui)
            {
                case UI.Text textUI:
                    var label = (Label)elem;
                    label.Content = textUI.Content;
                    break;

                case UI.Input inputUI:
                    var textBox = (TextBox)elem;
                    textBox.Text = inputUI.Content;
                    break;

                case UI.Button buttonUI:
                    var button = (Button)elem;
                    button.Content = buttonUI.Content;
                    break;

                case UI.CheckBox checkBoxUI:
                    var checkBox = (CheckBox)elem;
                    checkBox.Content = checkBoxUI.Content;
                    checkBox.IsChecked = checkBoxUI.IsChecked;
                    break;
            }
        }
    }
}
