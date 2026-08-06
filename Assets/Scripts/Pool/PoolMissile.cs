public class PoolMissile : Pool
{
    public static PoolMissile instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

}
