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
        public static ImList<Prop> props(params Prop[] ps) => ImToolsExt.FromArray(ps);

        public static Prop width(int n) => new Prop.Width(n);
        public static Prop height(int n) => new Prop.Height(n);
        public static Prop isEnabled(bool enabled) => enabled ? Prop.IsEnabled.Enabled : Prop.IsEnabled.Disabled;
        public static Prop tootip(string text) => new Prop.Tooltip(text);

        public static TProp Get<TProp>(this ImList<Prop> props, TProp defaultProp = null) where TProp : Prop => 
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

        public bool Equals(UI other) => 
            other != null && 
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
        public Func<TMsg, unit> TypedEvent;

        public UI(UI baseUi, Func<TMsg, unit> typedEvent)
        {
            BaseUI = baseUi;
            TypedEvent = typedEvent;
        }
    }

    /// Simple UI application, without commands and subscriptions.
    public class App<TMsg, TModel>
    {
        public readonly TModel Model;
        public readonly Func<TModel, TMsg, TModel> Update;
        public readonly Func<TModel, UI<TMsg>> View;

        public App(TModel model, Func<TModel, TMsg, TModel> update, Func<TModel, UI<TMsg>> view)
        {
            Model = model;
            Update = update;
            View = view;
        }
    }

    public interface INativeUI
    {
        void Send(ImList<UIUpdate> uiUpdates);
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
            ev.Value.Swap(s => ui.TypedEvent(changed(s)));
            return ui;
        }

        public static UI<TMsg> button<TMsg>(string text, TMsg clicked, 
            ImList<Prop> props = null)
        {
            var ev = Event.Of<unit>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.Button(props, text, ev), unit.Ignore);
            ev.Value.Swap(_ => ui.TypedEvent(clicked));
            return ui;
        }

        public static UI<TMsg> checkbox<TMsg>(string text, bool isChecked, Func<bool, TMsg> changed, 
            ImList<Prop> props = null)
        {
            var ev = Event.Of<bool>(unit.Ignore);
            var ui = new UI<TMsg>(new UI.CheckBox(props, text, isChecked, ev), unit.Ignore);
            ev.Value.Swap(check => ui.TypedEvent(changed(check)));
            return ui;
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, params UI<TMsg>[] parts)
        {
            return panel(layout, null, parts);
        }

        public static UI<TMsg> panel<TMsg>(Layout layout, 
            ImList<Prop> props = null, params UI<TMsg>[] parts)
        {
            var uiParts = ImList<UI>.Empty;

            // add in reverse order to preserve the parts order
            for (var i = parts.Length - 1; i >= 0; i--)
                uiParts = uiParts.Prep(parts[i].BaseUI);

            var ui = new UI<TMsg>(new UI.Panel(props, layout, uiParts), unit.Ignore);

            Func<TMsg, unit> raise = msg => ui.TypedEvent(msg);
            for (var i = 0; i < parts.Length; i++)
                parts[i].TypedEvent = raise;

            return ui;
        }
    }

    public static class UIApp
    {
        // todo: May be combined with the view to don't write it each time
        /// Returns a new UI component mapping the message event using the given function.
        public static UI<TMsg> MapMsg<TSubMsg, TMsg>(this UI<TSubMsg> source, Func<TSubMsg, TMsg> map)
        {
            var result = new UI<TMsg>(source.BaseUI, unit.Ignore);
            source.TypedEvent = msg => result.TypedEvent(map(msg));
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

        // Returns a UI application from a UI model, update and view.
        public static App<TMsg, TModel> App<TMsg, TModel>(
            TModel model,
            Func<TModel, TMsg, TModel> update,
            Func<TModel, UI<TMsg>> view)
        {
            return new App<TMsg, TModel>(model, update, view);
        }

        // Runs a UI application given a native UI.
        public static void Run<TMsg, TModel>(INativeUI nativeUI, App<TMsg, TModel> app)
        {
            Func<TModel, UI<TMsg>, TMsg, unit> handleRecursively = null;
            handleRecursively = (model, view, message) =>
            {
                var newModel = app.Update(model, message);
                var newUI = app.View(newModel);

                newUI.TypedEvent = msg => handleRecursively(newModel, newUI, msg);

                var uiUpdates = view.Diff(newUI);
                uiUpdates.To(unit._, (update, _) => (update as UIUpdate.Event)?.Raise(unit._));

                nativeUI.Send(uiUpdates);

                return unit._;
            };

            // Render and insert intial UI from the model
            var initialUI = app.View(app.Model);
            initialUI.TypedEvent = msg => handleRecursively(app.Model, initialUI, msg);
            nativeUI.Send(ImList<UIUpdate>.Empty.Prep(new UIUpdate.Insert(ImList<int>.Empty, initialUI.BaseUI)));
        }
    }
}
