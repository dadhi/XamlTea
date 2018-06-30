namespace Tea.Sample
{
    using static UIElements;
    using M = IMessage<ChangedEventSample>;

    public class ChangedEventSample : IComponent<ChangedEventSample>
    {
        public readonly string Text;
        public ChangedEventSample(string text = "") => Text = text;

        public override string ToString() => $"'{Text}'";

        public class Toggle : M
        {
            public static M Plus = new Toggle();
            public static M Minus = new Toggle();

            public override string ToString() => this == Plus ? "TogglePlus" : "ToggleMinus";
        }

        public ChangedEventSample Update(M message) =>
            new ChangedEventSample(message == Toggle.Minus ? Text + "-" : Text + "+");

        public UI<M> View() =>
            row(text<M>(Text),
                Text.EndsWith("++") 
                    ? button("Minus", Toggle.Minus) 
                    : button("Plus", Toggle.Plus));
    }
}
