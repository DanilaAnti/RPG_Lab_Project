using System;

namespace RPG_Lab_Project
{
    public enum CharacterClass
    {
        Warrior,
        Mage,
        Archer,
        Tank
    }

    public enum StatusEffect
    {
        Normal,
        Burning,
        Shielded
    }

    public interface IDefendable
    {
        void Defend(int damage);
    }

    public interface ISkillUser
    {
        void UseSkill(Character target);
    }

    public class Loot
    {
        public string Name { get; }
        public int Value { get; }

        public Loot(string name, int value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Loot name cannot be empty.");

            if (value < 0)
                throw new ArgumentOutOfRangeException("value", "Loot value cannot be negative.");

            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name + " (" + Value + " gold)";
        }
    }

    public class CharacterDeadException : Exception
    {
        private string msg;

        public CharacterDeadException(string m)
        {
            msg = m;
        }

        public override string Message
        {
            get { return "Game error: " + msg; }
        }
    }

    public abstract class Character : IDefendable, ISkillUser
    {
        private static int idCounter = 1;

        public int Id { get; }
        public string Name { get; protected set; }
        public int Health { get; protected set; }
        public int Mana { get; protected set; }
        public int Damage { get; protected set; }

        public CharacterClass ClassType { get; protected set; }
        public StatusEffect Status { get; protected set; }

        protected Random rnd;

        public bool IsAlive
        {
            get { return Health > 0; }
        }

        protected Character(string name, int hp, int mana, int dmg, CharacterClass type, Random r)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Character name cannot be empty.");

            if (hp <= 0)
                throw new ArgumentOutOfRangeException("hp", "Health must be greater than zero.");

            if (mana < 0)
                throw new ArgumentOutOfRangeException("mana", "Mana cannot be negative.");

            if (dmg < 0)
                throw new ArgumentOutOfRangeException("dmg", "Damage cannot be negative.");

            if (r == null)
                throw new ArgumentNullException("r", "Random generator cannot be null.");

            Id = idCounter++;
            Name = name;
            Health = hp;
            Mana = mana;
            Damage = dmg;
            ClassType = type;
            Status = StatusEffect.Normal;
            rnd = r;
        }

        public void EnsureAlive()
        {
            if (!IsAlive)
                throw new CharacterDeadException(Name + " already dead");
        }

        protected void EnsureTarget(Character target)
        {
            if (target == null)
                throw new ArgumentNullException("target", "Target cannot be null.");

            target.EnsureAlive();
        }

        public abstract void Attack(Character target);
        public abstract void UseSkill(Character target);

        public virtual void Defend(int damage)
        {
            EnsureAlive();

            if (damage < 0)
                throw new ArgumentOutOfRangeException("damage", "Damage cannot be negative.");

            if (Status == StatusEffect.Shielded)
            {
                damage -= 5;
                if (damage < 0)
                    damage = 0;

                Console.WriteLine(Name + " blocks part of damage with shield.");
                Status = StatusEffect.Normal;
            }

            Health -= damage;
            Console.WriteLine(Name + " takes " + damage + " damage. HP=" + Health);

            if (Health <= 0)
                Console.WriteLine(Name + " died.");
        }

        public override string ToString()
        {
            return ClassType + " " + Name + " (HP:" + Health + " MP:" + Mana + ", Status:" + Status + ")";
        }

        public override bool Equals(object obj)
        {
            Character c = obj as Character;
            if (c == null) return false;
            return c.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class Warrior : Character
    {
        public Warrior(string name, Random r) : base(name, 120, 30, 20, CharacterClass.Warrior, r) { }

        public override void Attack(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            int dmg = Damage + rnd.Next(5);
            Console.WriteLine(Name + " strikes " + target.Name + " with sword.");
            target.Defend(dmg);
        }

        public override void UseSkill(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            if (Mana < 10)
            {
                Console.WriteLine(Name + " lacks mana.");
                return;
            }

            Mana -= 10;
            Console.WriteLine(Name + " uses Power Strike!");
            target.Defend(Damage + 25);
        }
    }

    public class Mage : Character
    {
        public Mage(string name, Random r) : base(name, 80, 100, 15, CharacterClass.Mage, r) { }

        public override void Attack(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            int dmg = Damage + rnd.Next(6);
            Console.WriteLine(Name + " casts magic bolt at " + target.Name + ".");
            target.Defend(dmg);
        }

        public override void UseSkill(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            if (Mana < 20)
            {
                Console.WriteLine(Name + " lacks mana.");
                return;
            }

            Mana -= 20;
            Console.WriteLine(Name + " casts Fireball!");
            target.Defend(Damage + 30);
        }
    }

    public class Archer : Character
    {
        public Archer(string name, Random r) : base(name, 90, 40, 18, CharacterClass.Archer, r) { }

        public override void Attack(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            int dmg = Damage + rnd.Next(8);
            Console.WriteLine(Name + " shoots arrow at " + target.Name + ".");
            target.Defend(dmg);
        }

        public override void UseSkill(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            if (Mana < 15)
            {
                Console.WriteLine(Name + " lacks mana.");
                return;
            }

            Mana -= 15;
            Console.WriteLine(Name + " performs precise shot!");
            target.Defend(Damage + 20);
        }
    }

    public class Tank : Character
    {
        public Tank(string name, Random r) : base(name, 160, 20, 12, CharacterClass.Tank, r) { }

        public override void Attack(Character target)
        {
            EnsureAlive();
            EnsureTarget(target);

            Console.WriteLine(Name + " rams " + target.Name + ".");
            target.Defend(Damage + rnd.Next(4));
        }

        public override void UseSkill(Character target)
        {
            EnsureAlive();

            if (Mana < 5)
            {
                Console.WriteLine(Name + " lacks mana.");
                return;
            }

            Mana -= 5;
            Status = StatusEffect.Shielded;
            Console.WriteLine(Name + " raises shield!");
        }
    }

    class Program
    {
        static void Main()
        {
            Random rnd = new Random();

            Character[] party =
            {
                new Warrior("Thorin", rnd),
                new Mage("Gandalf", rnd),
                new Archer("Legolas", rnd),
                new Tank("Boromir", rnd)
            };

            Loot reward = new Loot("Ancient chest", 150);

            Console.WriteLine("Starting RPG battle simulation with updated balance...");
            Console.WriteLine("Possible reward: " + reward);

            Console.WriteLine("\n=== Characters ===");
            foreach (Character c in party)
                Console.WriteLine(c);

            Console.WriteLine("\n=== Battle simulation ===");

            for (int round = 1; round <= 5; round++)
            {
                Console.WriteLine("\nRound " + round);

                Character attacker = party[rnd.Next(party.Length)];
                Character target = party[rnd.Next(party.Length)];

                if (attacker == target)
                {
                    Console.WriteLine("The character decided to skip the turn.");
                    continue;
                }

                try
                {
                    if (rnd.Next(2) == 0)
                        attacker.Attack(target);
                    else
                        attacker.UseSkill(target);
                }
                catch (CharacterDeadException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("Validation error: " + e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected error: " + e.Message);
                }
            }

            Console.WriteLine("\n=== Final state ===");
            foreach (Character c in party)
                Console.WriteLine(c);

            Console.WriteLine("\nSimulation finished.");
            Console.ReadLine();
        }
    }
}