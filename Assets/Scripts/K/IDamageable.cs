public interface IDamageable
{
    public int Hp { get; set; }
    public int GroggyHp { get; set; }
    public int Barrier { get; set; }

    public void TakeDamage(int Damage);
}