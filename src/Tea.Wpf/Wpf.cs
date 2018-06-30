using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;
using static Tea.UIChange;

namespace Tea.Wpf
{
    public class WpfUI : INativeUI
    {
        /// <summary>Initializes the UI bound to provided content control.</summary>
        public static INativeUI Create(ContentControl root) => new WpfUI(root);

        private readonly ContentControl _rootControl;
        private WpfUI(ContentControl rootControl) => _rootControl = rootControl;

        /// <summary>Applies the updates.</summary>
        public void ApplyChanges(ImList<UIChange> changes) =>
            changes.Apply(x => _rootControl.Dispatcher.Invoke(() => Apply(x, _rootControl)));

        private static void Apply(UIChange change, ContentControl root)
        {
            var (pos, tail, isEmpty) = change.Path;
            switch (change)
            {
                case Insert insert:
                    if (isEmpty)
                        root.Content = CreateUI(insert.UI);
                    else
                        Locate(tail, root).Children.Insert(pos, CreateUI(insert.UI));
                    break;

                case Update update:
                    var elem = isEmpty ? (UIElement)root.Content : Locate(tail, root).Children[pos];
                    Update(update.UI, elem);
                    break;

                case Replace replace:
                    if (isEmpty)
                        root.Content = CreateUI(replace.UI);
                    else
                    {
                        var children = Locate(tail, root).Children;
                        children.RemoveAt(pos);
                        children.Insert(pos, CreateUI(replace.UI));
                    }
                    break;

                case Remove _:
                    if (!isEmpty)
                        Locate(tail, root).Children.RemoveAt(pos);
                    break;

                // todo: Leaky API, how do I know that Event is handled elsewhere
                case UIChange.Message _:
                    // do nothing for events because they are raised before applying update in main MVU loop
                    break;
            }
        }

        private static Panel Locate(ImList<int> path, ContentControl root) =>
            path.IsEmpty ? (Panel)root.Content : (Panel)Locate(path.Tail, root).Children[path.Head];

        private static UIElement CreateUI(UI ui)
        {
            if (ui == null)
                throw new ArgumentNullException(nameof(ui));

            if (ui is UI.Text text)
                return new Label { Content = text.Content };

            if (ui is UI.Input input)
            {
                var elem = new TextBox { Text = input.Content };
                var m = input.Changed;
                elem.TextChanged += (sender, _) => m.Send(((TextBox)sender).Text);
                return elem;
            }

            if (ui is UI.Button b)
            {
                var elem = new Button { Content = b.Label };
                var m = b.Clicked;
                elem.Click += (sender, _) => m.Send(Unit.unit);
                return elem;
            }

            if (ui is UI.Panel panel)
            {
                var orientation = panel.Layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                var elem = new StackPanel { Orientation = orientation };

                var kids = panel.Elements.Map(CreateUI);
                kids.Apply(x => elem.Children.Add(x));
                return elem;
            }

            if (ui is UI.Check check)
            {
                var elem = new CheckBox
                {
                    Content = check.Label,
                    IsChecked = check.IsChecked,
                };

                var m = check.Changed;
                elem.Checked += (sender, _) => m.Send(true);
                elem.Unchecked += (sender, _) => m.Send(false);

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
                case UI.Text t when elem is Label l:
                    l.Content = t.Content;
                    break;

                case UI.Input i when elem is TextBox tb:
                    tb.Text = i.Content;
                    break;

                case UI.Button b when elem is Button bt:
                    bt.Content = b.Label;
                    break;

                case UI.Check c when elem is CheckBox cb:
                    cb.Content = c.Label;
                    cb.IsChecked = c.IsChecked;
                    break;
            }
        }
    }
}
