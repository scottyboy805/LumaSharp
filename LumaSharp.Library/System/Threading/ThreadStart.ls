namespace System.Threading;

public delegate void ThreadStart();

public delegate void ThreadStart<T>(T argument);
