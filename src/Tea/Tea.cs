using System;
using ImTools;
#pragma warning disable 659

namespace Tea
{
    public static class Event
    {
        public static Ref<Ref<Func<TMsg, unit>>> Of<TMsg>(Func<TMsg, unit> evt)
        {
            return Ref.Of(Ref.Of(evt));
        }
    }

    public enum Layout { Horizontal, Vertical }

    public abstract class Prop
    {
        public class Of<TValue> : Prop
        {
            public readonly TValue Value;
            public Of(TValue value) { Value = value; }
            public override bool Equals(object obj)
            {
                var other = obj as Of<TValue>;
                return other != null && Equals(other.Value, Value);
            } 
        }

        public class Width : Of<int> { public Width(int value) : base(value) { } }
        public class Height : Of<int> { public Height(int value) : base(value) { } }

        public class IsEnabled : Of<bool>
        {
            public static readonly IsEnabled Enabled = new IsEnabled(true);
            public static readonly IsEnabled Disabled = new IsEnabled(false);
            private IsEnabled(bool value) : base(value) { }
        }
        public class Tooltip : Of<string> { public Tooltip(string value) : base(value) { } }
    }

    public static class Props
    {
        public static ImList<Prop> props(params Prop[] ps) => ImToolsExt.List(ps);

        public static Prop width(int n) => new Prop.Width(n);
        public static Prop height(int n) => new Prop.Height(n);
        public static Prop isEnabled(bool enabled) => enabled ? Prop.IsEnabled.Enabled : Prop.IsEnabled.Disabled;
        public static Prop tootip(string text) => new Prop.Tooltip(text);

        public static TProp Get<TProp>(this ImList<Prop> props, TProp defaultProp = null) 
            where TProp : Prop => 
            props.GetOrDefault(p => p is TProp) as TProp ?? defaultProp;

        public static ImList<Pair<Prop, Prop>> Diff(this ImList<Prop> it, ImList<Prop> other)
        {
            if (it.IsEmpty && other.IsEmpty)
                return ImList<Pair<Prop, Prop>>.Empty;
            if (other.IsEmpty)
                return it.Map(prop => pair.of<Prop, Prop>(prop, null));
            if (it.IsEmpty)
                return other.Map(prop => pair.of<Prop, Prop>(null, prop));
            var result = it.To(ImList<Pair<Prop, Prop>>.Empty, (p1, _) =>
            {
                Prop p2 = null;
                other = other.To(ImList<Prop>.Empty, (p, __) =>
                {
                    if (p.GetType() != p1.GetType())
                        return __.Prep(p);
                    p2 = p;
                    return __;
                });

                if (p2 == null)
                    return _.Prep(pair.of<Prop, Prop>(p1, null));

                if (p2 != null && p1 != p2 && !p2.Equals(p1))
                    return _.Prep(pair.of(p1, p2));

                return _;
            });
            if (other.IsEmpty)
                return result;
            return other.To(result, (p, _) => _.Prep(pair.of<Prop, Prop>(null, p)));
        }
    }

    public abstract class UI
    {
        public readonly ImList<Prop> Props;
        public readonly string Content;

        private UI(ImList<Prop> props, string content)
        {
            Props = props ?? ImList<Prop>.Empty;
            Content = content;
        }

        public virtual bool Equals(UI other) => 
            other != null && 
            other.GetType() == GetType() &&
            other.Content == Content &&
            Props.Diff(other.Props).IsEmpty;

        public class Text : UI
        {
            public Text(ImList<Prop> props, string content) : base(props, content) { }
        }

        public class Input : UI
        {
            public readonly Ref<Ref<Func<string, unit>>> Changed;

            public Input(ImList<Prop> props, string text, Ref<Ref<Func<string, unit>>> changed) : base(props, text)
            {
                Changed = changed;
            }
        }

        public class Button : UI
        {
            public readonly Ref<Ref<Func<unit, unit>>> Clicked;

            public Button(ImList<Prop> props, string text, Ref<Ref<Func<unit, unit>>> clicked) : base(props, text)
            {
                Clicked = clicked;
            }
        }

        public class CheckBox : UI
        {
            public readonly bool IsChecked;
            public readonly Ref<Ref<Func<bool, unit>>> Changed;

            public CheckBox(ImList<Prop> props, string text, bool isChecked, Ref<Ref<Func<bool, unit>>> changed) : base(props, text)
            {
                IsChecked = isChecked;
                Changed = changed;
            }

            public override bool Equals(UI other) => 
                base.Equals(other) && ((CheckBox)other).IsChecked == IsChecked;
        }

        public class Panel : UI
        {
            public readonly Layout Layout;
            public readonly ImList<UI> Parts;

            public Panel(ImList<Prop> props, Layout layout, ImList<UI> parts) : base(props, null)
            {
                Layout = layout;
                Parts = parts;
            }

            public override bool Equals(UI other) => false; // todo: comparison here?
        }
    }

    /// UI component update and event redirection.
    public abstract class UIUpdate
    {
        public class Insert : UIUpdate
        {
            public readonly ImList<int> Path;
            public readonly UI UI;

            public Insert(ImList<int> path, UI ui)
            {
                Path = path;
                UI = ui;
            }
        }

        public class Update : UIUpdate
        {
            public readonly ImList<int> Path;
            public readonly UI UI;

            public Update(ImList<int> path, UI ui)
            {
                Path = path;
                UI = ui;
            }
        }

        public class Replace : UIUpdate
        {
            public readonly ImList<int> Path;
            public readonly UI UI;

            public Replace(ImList<int> path, UI ui)
            {
                Path = path;
                UI = ui;
            }
        }

        public class Remove : UIUpdate
        {
            public readonly ImList<int> Path;

            public Remove(ImList<int> path)
            {
                Path = path;
            }
        }

        public class Event : UIUpdate
        {
            public readonly Func<unit, unit> Raise;

            public Event(Func<unit, unit> raise)
            {
                Raise = raise;
            }
        }
    }

    /// UI component including a message event.
    public class UI<TMsg>
    {
        public readonly UI BaseUI;
        public Func<TMsg, unit> OnMessage;

        public UI(UI baseUi, Func<TMsg, unit> onMessage)
        {
            BaseUI = baseUi;
            OnMessage = onMessage;
        }
    }

    /// <summary>Base interface for component with View only.</summary>
    public interface IComponent<TMsg>
    {
        UI<TMsg> View();
    }

    /// <summary>Base interface for component with Update, View but without Commands, Subscriptions.</summary>
    public interface IComponent<out TComponent, TMsg> : IComponent<TMsg>
        where TComponent : IComponent<TComponent, TMsg>
    {
        TComponent Update(TMsg msg);
    }

    public interface IMsg<TMsgType>
    {
        TMsgType Type { get; }
    }

    public class ItemChanged<TItemMsg, TMsgType> : IMsg<TMsgType>
    {
        public TMsgType Type { get; }
        public int Index { get; }
        public TItemMsg Msg { get; }

        public ItemChanged(int index, TItemMsg msg, TMsgType type)
        {
            Type = type;
            Index = index;
            Msg = msg;
        }
    }

    public static class ItemChanged
    {
        public static UI<IMsg<TMsgType>> View<TItemMsg, TMsgType>(this IComponent<TItemMsg> item, int i, TMsgType msgType)
        {
            return item.View().Wrap(msg => msg.Of(i, msgType));
        }

        public static IMsg<TMsgType> Of<TItemMsg, TMsgType>(this TItemMsg itemMsg, int i, TMsgType msgType)
        {
            return new ItemChanged<TItemMsg, TMsgType>(i, itemMsg, msgType);
        }
    }

    public interface INativeUI
    {
        void Apply(ImList<UIUpdate> uiUpdates);
    }

    public static class UIParts
    {
        public static UI<TMsg> text<TMsg>(string text, ImList<Prop> props = null)
        {
            return new UI<TMsg>(new UI.Text(props, text), unit.Ignore);
        }

        public static UI<TMsg> input<TMsg>(string text, Func<string, TMsg> changed, 
            ImList<Prop> props = null)
        {
            var ev = Event.Of<string>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.Input(props, text, ev), unit.Ignore);
            ev.Value.Swap(s => ui.OnMessage(changed(s)));
            return ui;
        }

        public static UI<TMsg> button<TMsg>(string text, TMsg clicked, 
            ImList<Prop> props = null)
        {
            var ev = Event.Of<unit>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.Button(props, text, ev), unit.Ignore);
            ev.Value.Swap(_ => ui.OnMessage(clicked));
            return ui;
        }

        public static UI<TMsg> checkbox<TMsg>(string text, bool isChecked, Func<bool, TMsg> changed, 
            ImList<Prop> props = null)
        {
            var ev = Event.Of<bool>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.CheckBox(props, text, isChecked, ev), unit.Ignore);
            ev.Value.Swap(check => ui.OnMessage(changed(check)));
            return ui;
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, params UI<TMsg>[] parts)
        {
            return panel(layout, null, parts);
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, ImList<UI<TMsg>> parts)
        {
            return panel(layout, null, parts.ToArray());
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, 
            ImList<Prop> props = null, params UI<TMsg>[] parts)
        {
            var uiParts = ImList<UI>.Empty;

            // add in reverse order to preserve the parts order
            for (var i = parts.Length - 1; i >= 0; i--)
                uiParts = uiParts.Prep(parts[i].BaseUI);

            var ui = new UI<TMsg>(new UI.Panel(props, layout, uiParts), unit.Ignore);

            unit Raise(TMsg msg) => ui.OnMessage(msg);
            for (var i = 0; i < parts.Length; i++)
                parts[i].OnMessage = Raise;

            return ui;
        }
    }

    public static class UIApp
    {
        // todo: May be combined with the view to avoid repeating in each parent component
        /// Returns a new UI component mapping the message event using the given function.
        public static UI<TMsg> Wrap<TItemMsg, TMsg>(this UI<TItemMsg> source, Func<TItemMsg, TMsg> map)
        {
            var result = new UI<TMsg>(source.BaseUI, unit.Ignore);
            source.OnMessage = msg => result.OnMessage(map(msg));
            return result;
        }

        /// Returns a list of UI updates from two UI components.
        public static ImList<UIUpdate> Diff<TMsg1, TMsg2>(this UI<TMsg1> oldUI, UI<TMsg2> newUI)
        {
            return Diff(oldUI.BaseUI, newUI.BaseUI, ImList<int>.Empty, 0, ImList<UIUpdate>.Empty);
        }

        private static ImList<UIUpdate> Diff(UI oldUI, UI newUI, ImList<int> path, int index, ImList<UIUpdate> diffs)
        {
            if (ReferenceEquals(oldUI, newUI))
                return diffs;

            // todo: consolidate same ui handling because it is not so different
            if (oldUI is UI.Text && newUI is UI.Text)
            {
                if (oldUI.Content != newUI.Content)
                    diffs = diffs.Prep(new UIUpdate.Update(path, newUI));
                return diffs;
            }

            if (oldUI is UI.Button && newUI is UI.Button)
            {
                // todo: for all
                if (!oldUI.Equals(newUI))
                    diffs = diffs.Prep(new UIUpdate.Update(path, newUI));

                var updateEvent = UpdateEvent(((UI.Button)oldUI).Clicked, ((UI.Button)newUI).Clicked);
                return diffs.Prep(new UIUpdate.Event(updateEvent));
            }

            if (oldUI is UI.Input && newUI is UI.Input)
            {
                if (oldUI.Content != newUI.Content)
                    diffs = diffs.Prep(new UIUpdate.Update(path, newUI));

                var updateEvent = UpdateEvent(((UI.Input)oldUI).Changed, ((UI.Input)newUI).Changed);
                return diffs.Prep(new UIUpdate.Event(updateEvent));
            }

            var oldDiv = oldUI as UI.Panel;
            var newDiv = newUI as UI.Panel;

            if (oldDiv != null && newDiv != null)
            {
                // if layout changed then fully replace
                if (oldDiv.Layout != newDiv.Layout)
                    return diffs.Prep(new UIUpdate.Replace(path, newDiv));

                var oldParts = oldDiv.Parts;
                var newParts = newDiv.Parts;

                // if both empty
                if (oldParts.IsEmpty && newParts.IsEmpty)
                    return diffs;

                // for each new child UI do insert
                if (oldParts.IsEmpty)
                    return newParts.To(diffs,
                        (ui, i, _) => _.Prep(new UIUpdate.Insert(path.Prep(index + i), ui)))
                        .Reverse();

                // remove old ui children
                if (newParts.IsEmpty)
                    return oldParts.To(diffs,
                        (ui, i, _) => _.Prep(new UIUpdate.Remove(path.Prep(index + i))));

                // diff the first items, then recursively the rest 
                diffs = Diff(oldParts.Head, newParts.Head, path.Prep(index), 0, diffs);

                return Diff(
                    new UI.Panel(oldDiv.Props, oldDiv.Layout, oldParts.Tail),
                    new UI.Panel(oldDiv.Props, oldDiv.Layout, newParts.Tail),
                    path, index + 1, diffs);
            }

            // otherwise just replace
            return diffs.Prep(new UIUpdate.Replace(path, newUI));
        }

        // Point first ref to second ref value.value
        private static Func<unit, unit> UpdateEvent<T>(Ref<Ref<T>> ref1, Ref<Ref<T>> ref2)
            where T : class
        {
            return _ =>
            {
                //let ev = !e1 in ev:=!(!e2); e2:=ev
                var ref1Value = ref1.Value;
                ref1Value.Swap(ref2.Value.Value);
                ref2.Swap(ref1Value);
                return unit._;
            };
        }

        // Runs a UI application given a native UI.
        public static void Run<TMsg, TModel>(INativeUI nativeUI, IComponent<TModel, TMsg> component) 
            where TModel : IComponent<TModel, TMsg>
        {
            unit UpdateViewLoop(IComponent<TModel, TMsg> model, UI<TMsg> ui, TMsg message)
            {
                var newModel = model.Update(message);
                var newUI = newModel.View();
                newUI.OnMessage = msg => UpdateViewLoop(newModel, newUI, msg);

                var uiUpdates = ui.Diff(newUI);
                uiUpdates.To(unit._, (update, _) => (update as UIUpdate.Event)?.Raise(unit._));

                nativeUI.Apply(uiUpdates);

                return unit._;
            }

            // Render and insert intial UI from the model
            var initialUI = component.View();
            initialUI.OnMessage = msg => UpdateViewLoop(component, initialUI, msg);
            nativeUI.Apply(ImList<UIUpdate>.Empty.Prep(new UIUpdate.Insert(ImList<int>.Empty, initialUI.BaseUI)));
        }
    }
}
