using System;
using System.Windows;
using System.Windows.Controls;
using ImTools;
using Pnl = System.Windows.Controls.Panel;
using Btn = System.Windows.Controls.Button;

namespace Tea.Wpf
{
    public class WpfUI : INativeUI
    {
        /// <summary>Initializes the UI bound to provided content control.</summary>
        public static INativeUI Create(ContentControl root) => new WpfUI(root);

        private readonly ContentControl _rootControl;
        private WpfUI(ContentControl rootControl) => _rootControl = rootControl;

        /// <summary>Applies the updates.</summary>
        public void ApplyPatches(ImList<Patch.union> changes) =>
            changes.ForEach(x => _rootControl.Dispatcher.Invoke(() => Apply(x, _rootControl)));

        private static void Apply(Patch.union change, ContentControl root)
        {
            switch (change)
            {
                case Is<Insert> insert:
                    {
                        var ((pos, tail, isEmpty), ui) = insert.Value();
                        if (isEmpty)
                            root.Content = CreateElement(ui);
                        else
                            Locate(tail, root).Children.Insert(pos, CreateElement(ui));
                        break;
                    }
                case Is<Update> update:
                    {
                        var ((pos, tail, isEmpty), ui) = update.Value();
                        var elem = isEmpty ? (UIElement)root.Content : Locate(tail, root).Children[pos];
                        Update(ui, elem);
                        break;
                    }
                case Is<Replace> replace:
                    {
                        var ((pos, tail, isEmpty), ui) = replace.Value();
                        var elem = CreateElement(ui);
                        if (isEmpty)
                            root.Content = elem;
                        else
                        {
                            var children = Locate(tail, root).Children;
                            children.RemoveAt(pos);
                            children.Insert(pos, elem);
                        }
                        break;
                    }
                case Is<Remove> remove:
                    {
                        var (pos, tail, isEmpty) = remove.Value();
                        if (!isEmpty)
                            Locate(tail, root).Children.RemoveAt(pos);
                        break;
                    }
                // todo: Leaky API, how do I know that Event is handled elsewhere
                case Is<Event> _:
                    // do nothing for events because they are raised before applying update in main MVU loop
                    break;
            }
        }

        private static Pnl Locate(ImList<int> path, ContentControl root) =>
            path.IsEmpty ? (Pnl)root.Content : (Pnl)Locate(path.Tail, root).Children[path.Head];

        private static UIElement CreateElement(UI.union ui)
        {
            switch (ui)
            {
                case Is<Text> text:
                    return new Label { Content = text.Value() };

                case Is<Input> input:
                    {
                        var (content, changed) = input.Value();
                        var tb = new TextBox { Text = content };
                        tb.TextChanged += (sender, _) => changed.Send(((TextBox)sender).Text);
                        return tb;
                    }
                case Is<Button> button:
                    {
                        var (label, clicked) = button.Value();
                        var b = new Btn { Content = label };
                        b.Click += (sender, _) => clicked.Send(Empty.Value);
                        return b;
                    }
                case Is<Panel> panel:
                    {
                        var (layout, elems) = panel.Value();
                        var orientation = layout == Layout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
                        var p = new StackPanel { Orientation = orientation };
                        elems.Map(CreateElement).Map(e => p.Children.Add(e));
                        return p;
                    }
                case Is<Check> check:
                    {
                        var (label, isChecked, changed) = check.Value();
                        var c = new CheckBox { Content = label, IsChecked = isChecked };
                        c.Checked += (s, _) => changed.Send(true);
                        c.Unchecked += (s, _) => changed.Send(false);
                        return c;
                    }
                default:
                    throw new NotSupportedException("The type of UI is not supported: " + ui.GetType());
            }
        }

        private static void Update(UI.union ui, UIElement elem)
        {
            switch (ui)
            {
                case Is<Text> t when elem is Label l:
                    l.Content = t.Value;
                    break;

                case Is<Input> i when elem is TextBox tb:
                    (tb.Text, _) = i.Value();
                    break;

                case Is<Button> b when elem is Btn bt:
                    (bt.Content, _) = b.Value();
                    break;

                case Is<Check> c when elem is CheckBox cb:
                    (cb.Content, cb.IsChecked, _) = c.Value();
                    break;
            }
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
    }
}
