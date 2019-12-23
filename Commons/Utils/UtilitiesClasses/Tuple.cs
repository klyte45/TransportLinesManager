namespace Klyte.Commons.Utils
{

    public class Tuple<T1, T2, T3, T4>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public T3 Third { get; private set; }
        public T4 Fourth { get; private set; }
        internal Tuple(ref T1 first, ref T2 second, ref T3 third, ref T4 fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }
    }

    public class Tuple<T1, T2, T3>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public T3 Third { get; private set; }
        internal Tuple(ref T1 first, ref T2 second, ref T3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }

    public class Tuple<T1, T2>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        internal Tuple(ref T1 first, ref T2 second)
        {
            First = first;
            Second = second;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2, T3, T4> New<T1, T2, T3, T4>(T1 first, T2 second, T3 third, T4 fourth)
        {
            Tuple<T1, T2, T3, T4> tuple = new Tuple<T1, T2, T3, T4>(ref first, ref second, ref third, ref fourth);
            return tuple;
        }
        public static Tuple<T1, T2, T3> New<T1, T2, T3>(T1 first, T2 second, T3 third)
        {
            Tuple<T1, T2, T3> tuple = new Tuple<T1, T2, T3>(ref first, ref second, ref third);
            return tuple;
        }
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            Tuple<T1, T2> tuple = new Tuple<T1, T2>(ref first, ref second);
            return tuple;
        }
        public static Tuple<T1, T2, T3, T4> NewRef<T1, T2, T3, T4>(T1 first, T2 second, T3 third, T4 fourth)
        {
            Tuple<T1, T2, T3, T4> tuple = new Tuple<T1, T2, T3, T4>(ref first, ref second, ref third, ref fourth);
            return tuple;
        }
        public static Tuple<T1, T2, T3> NewRef<T1, T2, T3>(ref T1 first, ref T2 second, ref T3 third)
        {
            Tuple<T1, T2, T3> tuple = new Tuple<T1, T2, T3>(ref first, ref second, ref third);
            return tuple;
        }
        public static Tuple<T1, T2> NewRef<T1, T2>(ref T1 first, ref T2 second)
        {
            Tuple<T1, T2> tuple = new Tuple<T1, T2>(ref first, ref second);
            return tuple;
        }
    }
}
