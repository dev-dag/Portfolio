public static class WeaponFactory
{
    public static Weapon CreateWeapon(WeaponEnum weaponType)
    {
        switch (weaponType)
        {
            case WeaponEnum.Sword:
                return new Sword();
            case WeaponEnum.Dagger:
                return new Dagger();
            case WeaponEnum.Axe:
                return new Axe();
            default:
                EDebug.LogError("Invalid weapon type");
                return null;
        }
    }
}
