// ------------------------------------- Simple singleton
public class lwSingleton<T> where T : new()
{
	public static T instance 
	{
		get
		{
			if( s_instance==null )
				s_instance = new T();
			return s_instance;
		}
	}

	private static T s_instance;

	public static void DestroyInstance()
	{
		s_instance = default(T);
	}

	public static bool IsInstanceValid()
	{
		return s_instance!=null;
	}
}
