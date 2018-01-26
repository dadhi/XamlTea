using System;
using ImTools;
// ReSharper disable InconsistentNaming
#pragma warning disable 659

namespace Tea
{
    public static class Event
    {
        public static void Empty<TMsg>(TMsg _) { }
        public static Ref<Ref<Action<TMsg>>> Of<TMsg>(Action<TMsg> send) => Ref.Of(Ref.Of(send));
    }

    public enum Layout { Horizontal, Vertical }

    //public abstract class Style
    //{
    //    public class Of<TValue> : Style
    //    {
    //        public readonly TValue Value;
    //        public Of(TValue value) { Value = value; }
    //        public override bool Equals(object obj)
    //        {
    //            var other = obj as Of<TValue>;
    //            return other != null && Equals(other.Value, Value);
    //        }
    //    }

    //    public class Width : Of<int> { public Width(int value) : base(value) { } }
    //    public class Height : Of<int> { public Height(int value) : base(value) { } }

    //    public class IsEnabled : Of<bool>
    //    {
    //        public static readonly IsEnabled Enabled = new IsEnabled(true);
    //        public static readonly IsEnabled Disabled = new IsEnabled(false);
    //        private IsEnabled(bool value) : base(value) { }
    //    }
    //    public class Tooltip : Of<string> { public Tooltip(string value) : base(value) { } }
    //}

    //public static class Styles
    //{
    //    public static ImList<Style> style(params Style[] styles) => styles.AsList();

    //    public static Style width(int n) => new Style.Width(n);
    //    public static Style height(int n) => new Style.Height(n);
    //    public static Style isEnabled(bool enabled) => enabled ? Style.IsEnabled.Enabled : Style.IsEnabled.Disabled;
    //    public static Style tooltip(string text) => new Style.Tooltip(text);

    //    public static ImList<(Style, Style)> PrependDiffs(this ImList<Style> source, ImList<Style> other)
    //    {
    //        if (source.IsEmpty && other.IsEmpty)
    //            return ImList<(Style, Style)>.Empty;

    //        if (other.IsEmpty)
    //            return source.Map(s => (s, default(Style)));

    //        if (source.IsEmpty)
    //            return other.Map(s => (default(Style), s));

    //        var resultDiff = source.Fold(ImList<(Style, Style)>.Empty, (sourceStyle, sourceDiff) =>
    //        {
    //            var s2 = default(Style);
    //            other = other.Fold(ImList<Style>.Empty, (otherStyle, otherDiff) =>
    //            {
    //                if (otherStyle.GetType() != sourceStyle.GetType())
    //                    return otherDiff.Prepend(otherStyle);

    //                s2 = otherStyle;
    //                return otherDiff;
    //            });

    //            if (s2 == null)
    //                return sourceDiff.Prepend((sourceStyle, default(Style)));

    //            if (s2 != null && sourceStyle != s2 && !s2.Equals(sourceStyle))
    //                return sourceDiff.Prepend((sourceStyle, s2));

    //            return sourceDiff;
    //        });

    //        if (other.IsEmpty)
    //            return resultDiff;

    //        return other.Fold(resultDiff, (style, diff) => diff.Prepend((default(Style), style)));
    //    }
    //}

    public abstract class UI
    {
        public readonly string Content;
        private UI(string content) => Content = content;

        public class Text : UI
        {
            public Text(string text) : base(text) { }
        }

        public class Input : UI
        {
            public readonly Ref<Ref<Action<string>>> Changed;
            public Input(string input, Ref<Ref<Action<string>>> changed) : base(input) =>
                Changed = changed;
        }

        public class Button : UI
        {
            public readonly Ref<Ref<Action<Unit>>> Clicked;

            public Button(string label, Ref<Ref<Action<Unit>>> clicked) : base(label) => 
                Clicked = clicked;
        }

        public class CheckBox : UI
        {
            public readonly bool IsChecked;
            public readonly Ref<Ref<Action<bool>>> Changed;

            public CheckBox(string label, bool isChecked, Ref<Ref<Action<bool>>> changed) : base(label)
            {
                IsChecked = isChecked;
                Changed = changed;
            }
        }

        public class Panel : UI
        {
            public readonly Layout Layout;
            public readonly ImList<UI> Parts;

            public Panel(Layout layout, ImList<UI> parts) : base(null)
            {
                Layout = layout;
                Parts = parts;
            }
        }
    }

    /// UI component update and event redirection.
    public abstract class UIUpdate
    {
        public readonly ImList<int> Path;

        protected UIUpdate(ImList<int> path)
        {
            Path = path;
        }

        public class Insert : UIUpdate
        {
            public readonly UI UI;
            public Insert(ImList<int> path, UI ui) : base(path) => UI = ui;
        }

        public class Update : UIUpdate
        {
            public readonly UI UI;

            public Update(ImList<int> path, UI ui) : base(path) => UI = ui;
        }

        public class Replace : UIUpdate
        {
            public readonly UI UI;

            public Replace(ImList<int> path, UI ui) : base(path) => UI = ui;
        }

        public class Remove : UIUpdate
        {
            public Remove(ImList<int> path) : base(path) { }
        }

        public class Event : UIUpdate
        {
            public readonly Action<Unit> Send;
            public Event(Action<Unit> send) : base(ImList<int>.Empty) => Send = send;
        }
    }

    /// UI component including a message event.
    public class UI<TMsg>
    {
        public readonly UI BaseUI;
        public Action<TMsg> Send;

        public UI(UI baseUi, Action<TMsg> send)
        {
            BaseUI = baseUi;
            Send = send;
        }
    }

    /// <summary>Base interface for component with Update, View but without Commands, Subscriptions.</summary>
    public interface IComponent<T> where T : IComponent<T>
    {
        T Update(IMsg<T> msg);

        UI<IMsg<T>> View();
    }

    /// <summary>Marker interface to allow boilerplate removal.</summary>
    /// <typeparam name="T">Component type</typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IMsg<T> { }

    public class ItemChanged<TItem, THolder> : IMsg<THolder>
    {
        public int Index { get; }
        public IMsg<TItem> Msg { get; }

        public ItemChanged(int index, IMsg<TItem> msg)
        {
            Index = index;
            Msg = msg;
        }
    }

    public static class Component
    {
        public static UI<IMsg<THolder>> ViewIn<TItem, THolder>(this TItem item, int itemIndex)
            where TItem : IComponent<TItem>
            where THolder : IComponent<THolder> =>
            item.View().MapMsg(msg => msg.Lift<TItem, THolder>(itemIndex));

        public static UI<IMsg<THolder>> ViewIn<TItem, THolder>(this TItem item, THolder hereOnlyForTypeInference,
            int itemIndex = 0)
            where TItem : IComponent<TItem> => 
            item.View().MapMsg(msg => msg.Lift<TItem, THolder>(itemIndex));

        public static IMsg<THolder> Lift<TItem, THolder>(this IMsg<TItem> itemMsg, int itemIndex)
            => new ItemChanged<TItem, THolder>(itemIndex, itemMsg);
    }

    public interface INativeUI
    {
        void Apply(ImList<UIUpdate> uiUpdates);
    }

    public static class UIParts
    {
        public static UI<TMsg> text<TMsg>(string text) =>
            new UI<TMsg>(new UI.Text(text), Event.Empty);

        public static UI<TMsg> input<TMsg>(string text, Func<string, TMsg> changed)
        {
            var ev = Event.Of<string>(Event.Empty);
            var ui = new UI<TMsg>(new UI.Input(text, ev), Event.Empty);
            ev.Value.Swap(s => ui.Send(changed(s)));
            return ui;
        }

        public static UI<TMsg> button<TMsg>(string text, TMsg clicked)
        {
            var ev = Event.Of<Unit>(Event.Empty);
            var ui = new UI<TMsg>(new UI.Button(text, ev), Event.Empty);
            ev.Value.Swap(_ => ui.Send(clicked));
            return ui;
        }

        public static UI<TMsg> checkbox<TMsg>(string text, bool isChecked, Func<bool, TMsg> changed)
        {
            var ev = Event.Of<bool>(Event.Empty);
            var ui = new UI<TMsg>(new UI.CheckBox(text, isChecked, ev), Event.Empty);
            ev.Value.Swap(check => ui.Send(changed(check)));
            return ui;
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, ImList<UI<TMsg>> uis)
        {
            var panel = new UI<TMsg>(new UI.Panel(layout, uis.Map(x => x.BaseUI)), Event.Empty);
            uis.Do(x => x.Send = panel.Send);
            return panel;
        }

        public static UI<TMsg> row<TMsg>(ImList<UI<TMsg>> uis) =>
            panel(Layout.Horizontal, uis);

        public static UI<TMsg> row<TMsg>(params UI<TMsg>[] uis) => row(uis.list());

        public static UI<TMsg> column<TMsg>(ImList<UI<TMsg>> uis) =>
            panel(Layout.Vertical, uis);

        public static UI<TMsg> column<TMsg>(params UI<TMsg>[] uis) => column(uis.list());
    }

    public static class UIApp
    {
        ///<summary>Returns a new UI component mapping the message event using the given function.</summary>
        public static UI<THolderMsg> MapMsg<TItemMsg, THolderMsg>(
            this UI<TItemMsg> source, Func<TItemMsg, THolderMsg> map)
        {
            var result = new UI<THolderMsg>(source.BaseUI, Event.Empty);
            source.Send = msg => result.Send(map(msg));
            return result;
        }

        ///<summary>Returns a list of UI updates from two UI components.
        /// To ensure correct insert and removal sequence where the insert/remove index are existing.</summary> 
        public static ImList<UIUpdate> DiffToUpdates<TMsg1, TMsg2>(this UI<TMsg1> oldUI, UI<TMsg2> newUI) =>
            Diff(ImList<UIUpdate>.Empty, oldUI.BaseUI, newUI.BaseUI, path: ImList<int>.Empty, index: 0);

        private static ImList<UIUpdate> Diff(this ImList<UIUpdate> updates,
            UI oldUI, UI newUI, ImList<int> path, int index)
        {
            if (ReferenceEquals(oldUI, newUI))
                return updates;

            if (oldUI is UI.Text && newUI is UI.Text)
            {
                if (oldUI.Content != newUI.Content)
                    updates = updates.Prepend(new UIUpdate.Update(path, newUI));
                return updates;
            }

            if (oldUI is UI.Button oldButton && newUI is UI.Button newButton)
            {
                if (oldButton.Content != newButton.Content)
                    updates = updates.Prepend(new UIUpdate.Update(path, newButton));
                return updates.Prepend(new UIUpdate.Event(UpdateEvent(oldButton.Clicked, newButton.Clicked)));
            }

            if (oldUI is UI.Input oldInput && newUI is UI.Input newInput)
            {
                if (oldInput.Content != newInput.Content)
                    updates = updates.Prepend(new UIUpdate.Update(path, newInput));
                return updates.Prepend(new UIUpdate.Event(UpdateEvent(oldInput.Changed, newInput.Changed)));
            }

            if (oldUI is UI.CheckBox oldCheckbox && newUI is UI.CheckBox newCheckbox)
            {
                if (oldCheckbox.Content != newCheckbox.Content ||
                    oldCheckbox.IsChecked != newCheckbox.IsChecked)
                    updates = updates.Prepend(new UIUpdate.Update(path, newCheckbox));
                return updates.Prepend(new UIUpdate.Event(UpdateEvent(oldCheckbox.Changed, newCheckbox.Changed)));
            }

            if (oldUI is UI.Panel oldPanel && newUI is UI.Panel newPanel)
            {
                // if layout changed then fully replace
                if (oldPanel.Layout != newPanel.Layout)
                    return updates.Prepend(new UIUpdate.Replace(path, newPanel));

                var oldParts = oldPanel.Parts;
                var newParts = newPanel.Parts;

                // if both empty
                if (oldParts.IsEmpty && newParts.IsEmpty)
                    return updates;

                // for each new child UI do insert
                if (oldParts.IsEmpty)
                    return newParts.Fold(updates,
                        (ui, i, d) => d.Prepend(new UIUpdate.Insert(path.Prepend(index + i), ui)));

                // remove old ui children
                if (newParts.IsEmpty)
                    return oldParts.Fold(updates,
                        (ui, i, d) => d.Prepend(new UIUpdate.Remove(path.Prepend(index + i))));

                updates = updates.Diff(oldParts.Head, newParts.Head, path.Prepend(index), 0);
                if (oldParts.Tail.IsEmpty && newParts.Tail.IsEmpty)
                    return updates;

                // todo: optimize by removing the re-creation of Panel just for recursion
                return updates.Diff(
                    new UI.Panel(oldPanel.Layout, oldParts.Tail),
                    new UI.Panel(newPanel.Layout, newParts.Tail),
                    path, index + 1);
            }

            // otherwise just replace
            return updates.Prepend(new UIUpdate.Replace(path, newUI));
        }

        // Point first ref to second ref value.value
        private static Action<Unit> UpdateEvent<T>(Ref<Ref<T>> evt1, Ref<Ref<T>> evt2)
            where T : class
        {
            return _ =>
            {
                //let ev = !e1 in ev:=!(!e2); e2:=ev
                var ref1Value = evt1.Value;
                ref1Value.Swap(evt2.Value.Value);
                evt2.Swap(ref1Value);
            };
        }

        /// <summary>Runs Model-View-Update loop, e.g. Init->View->Update->View->Update->View... </summary>
        public static void Run<T>(INativeUI nativeUI, IComponent<T> component) where T : IComponent<T>
        {
            void UpdateViewLoop(IComponent<T> model, UI<IMsg<T>> ui, IMsg<T> message)
            {
                var newModel = model.Update(message);
                var newUI = newModel.View();
                newUI.Send = msg => UpdateViewLoop(newModel, newUI, msg);

                var uiUpdates = ui.DiffToUpdates(newUI);

                for (var items = uiUpdates; !items.IsEmpty; items = items.Tail)
                    (items.Head as UIUpdate.Event)?.Send(Unit.unit);

                nativeUI.Apply(uiUpdates);
            }

            // Render and insert initial UI from the model
            var initialUI = component.View();
            initialUI.Send = msg => UpdateViewLoop(component, initialUI, msg);
            nativeUI.Apply(ImList<UIUpdate>.Empty.Prepend(new UIUpdate.Insert(ImList<int>.Empty, initialUI.BaseUI)));
        }
    }
}
