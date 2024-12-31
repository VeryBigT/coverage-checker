using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static coverage_checker.Type;
using static coverage_checker.StringHelper;

using DualType = System.Tuple<coverage_checker.Type, coverage_checker.Type>;

namespace coverage_checker
{
	public enum Type : int
	{
		NORMAL, FIGHTING, FLYING, POISON, GROUND, ROCK, BUG, GHOST, FIRE, WATER, GRASS,
		ELECTRIC, PSYCHIC, ICE, DRAGON, STEEL, DARK, FAIRY, NO_TYPE = -1
	}

	public static class StringHelper
	{
		public static string GetString(string key)
		{
			string s = Application.Current.Resources[key] as string;
			return s.Length > 0 ? s : "MISSING TRANSLATION FOR KEY: " + key;
		}

#pragma warning disable CS8632
		public static string GetRegEx(string key, params object?[] args)
		{
			return string.Format(GetString(key), args);
		}
#pragma warning restore CS8632
	}

	public static class IteratorHelper
	{
		public static IEnumerable<List<T>> Subsets<T>(List<T> objects, int maxLength)
		{
			if (objects == null || maxLength <= 0)
			{
				yield break;
			}
			Stack<int> stack = new Stack<int>(maxLength);
			int i = 0;
			while (stack.Count > 0 || i < objects.Count)
			{
				if (i < objects.Count)
				{
					if (stack.Count == maxLength)
					{
						i = stack.Pop() + 1;
					}
					stack.Push(i++);
					yield return (from index in stack.Reverse()
								  select objects[index]).ToList();
				}
				else
				{
					i = stack.Pop() + 1;
					if (stack.Count > 0)
					{
						i = stack.Pop() + 1;
					}
				}
			}
		}

		public class AscendingDuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
		{
			public int Compare(TKey x, TKey y)
			{
				int result = x.CompareTo(y);
				return result == 0 ? 1 : result;
			}
		}

		public class DescendingDuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
		{
			public int Compare(TKey x, TKey y)
			{
				int result = -x.CompareTo(y);
				return result == 0 ? 1 : result;
			}
		}
	}

	public static class TypeMethods
	{

		public static Type[] TypesOfGen(int gen)
		{
			return gen == 1 ? Constants.TYPES_OF_GEN_1
				: gen < 6 ? Constants.TYPES_OF_GEN_2
				: Constants.TYPES_OF_GEN_6;
		}

		public static string DualTypeToString(DualType t)
		{
			return t.Item2 == NO_TYPE ? TypeToString(t.Item1) 
				: TypeToString(t.Item1) + "/" + TypeToString(t.Item2);
		}

		public static string TypeToString(Type t)
		{
			return t switch
			{
				NORMAL => GetString("normal"),
				FIGHTING => GetString("fighting"),
				FLYING => GetString("flying"),
				POISON => GetString("poison"),
				GROUND => GetString("ground"),
				ROCK => GetString("rock"),
				BUG => GetString("bug"),
				GHOST => GetString("ghost"),
				FIRE => GetString("fire"),
				WATER => GetString("water"),
				GRASS => GetString("grass"),
				ELECTRIC => GetString("electric"),
				PSYCHIC => GetString("psychic"),
				ICE => GetString("ice"),
				DRAGON => GetString("dragon"),
				STEEL => GetString("steel"),
				DARK => GetString("dark"),
				FAIRY => GetString("fairy"),
				NO_TYPE => "ERROR",
				_ => "ERROR"
			};
		}

		public static Type StringToType(string s)
		{
			return s switch
			{
				"normal" => NORMAL,
				"fighting" => FIGHTING,
				"flying" => FLYING,
				"poison" => POISON,
				"ground" => GROUND,
				"rock" => ROCK,
				"bug" => BUG,
				"ghost" => GHOST,
				"fire" => FIRE,
				"water" => WATER,
				"grass" => GRASS,
				"electric" => ELECTRIC,
				"psychic" => PSYCHIC,
				"ice" => ICE,
				"dragon" => DRAGON,
				"steel" => STEEL,
				"dark" => DARK,
				"fairy" => FAIRY,
				_ => NO_TYPE
			};
		}

		public static float TypeFactor(Type atk, Type def, int gen)
		{
			int idx = gen == 1 ? 0 : gen < 6 ? 1 : 2;
			return Constants.TYPE_TABLE[(atk, def)][idx];
		}

		public static float TypeFactor(Type atk, DualType def, int gen)
		{
			return def.Item2 == NO_TYPE ? TypeFactor(atk, def.Item1, gen)
				: TypeFactor(atk, def.Item1, gen) * TypeFactor(atk, def.Item2, gen);
		}

		internal static ISet<DualType> DualTypesOfGen(int gen)
		{
			ISet<DualType> result = new SortedSet<DualType>();
			Type[] types = TypesOfGen(gen);
			for (int i = 0; i < types.Length; ++i)
			{
				for (int j = i + 1; j < types.Length; ++j)
				{
					_ = result.Add(new DualType(types[i], types[j]));
				}
				_ = result.Add(new DualType(types[i], NO_TYPE));
			}
			return result;
		}

		internal static ISet<DualType> ActualDualTypesOfGen(int gen)
		{
			return gen switch
			{
				1 => Constants.DUAL_TYPES_OF_GEN_1,
				2 => Constants.DUAL_TYPES_OF_GEN_2,
				3 => Constants.DUAL_TYPES_OF_GEN_3,
				4 => Constants.DUAL_TYPES_OF_GEN_4,
				5 => Constants.DUAL_TYPES_OF_GEN_5,
				6 => Constants.DUAL_TYPES_OF_GEN_6,
				7 => Constants.DUAL_TYPES_OF_GEN_7,
				8 => Constants.DUAL_TYPES_OF_GEN_8,
				9 => Constants.DUAL_TYPES_OF_GEN_9,
				_ => Constants.DUAL_TYPES_OF_GEN_9
			};
		}
	}

	public static class Constants
	{
		public static readonly Dictionary<(Type, Type), float[]> TYPE_TABLE;

		public static readonly Type[] TYPES_OF_GEN_1 = { NORMAL, FIGHTING, FLYING, POISON, GROUND,
			ROCK, BUG, GHOST, FIRE, WATER, GRASS, ELECTRIC, PSYCHIC, ICE, DRAGON };

		public static readonly Type[] TYPES_OF_GEN_2 = { NORMAL, FIGHTING, FLYING, POISON, GROUND,
			ROCK, BUG, GHOST, FIRE, WATER, GRASS, ELECTRIC, PSYCHIC, ICE, DRAGON, STEEL,
			DARK };

		public static readonly Type[] TYPES_OF_GEN_6 = { NORMAL, FIGHTING, FLYING, POISON, GROUND,
			ROCK, BUG, GHOST, FIRE, WATER, GRASS, ELECTRIC, PSYCHIC, ICE, DRAGON, STEEL,
			DARK, FAIRY };

		//TODO const arrays für dualtypes for each gen (ufff)
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_1;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_2;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_3;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_4;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_5;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_6;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_7;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_8;
		public static readonly ISet<DualType> DUAL_TYPES_OF_GEN_9;

		static Constants()
		{
			DUAL_TYPES_OF_GEN_1 = new SortedSet<DualType>
			{
				new DualType(WATER, NO_TYPE),
				new DualType(NORMAL, NO_TYPE),
				new DualType(POISON, NO_TYPE),
				new DualType(FIRE, NO_TYPE),
				new DualType(POISON, GRASS),
				new DualType(NORMAL, FLYING),
				new DualType(FIGHTING, NO_TYPE),
				new DualType(PSYCHIC, NO_TYPE),
				new DualType(GROUND, NO_TYPE),
				new DualType(GROUND, ROCK),
				new DualType(ELECTRIC, NO_TYPE),
				new DualType(POISON, BUG),
				new DualType(ROCK, WATER),
				new DualType(POISON, GHOST),
				new DualType(BUG, NO_TYPE),
				new DualType(WATER, PSYCHIC),
				new DualType(WATER, ICE),
				new DualType(FLYING, POISON),
				new DualType(FLYING, BUG),
				new DualType(FLYING, FIRE),
				new DualType(POISON, GROUND),
				new DualType(POISON, WATER),
				new DualType(BUG, GRASS),
				new DualType(GRASS, PSYCHIC),
				new DualType(DRAGON, NO_TYPE),
				new DualType(FIGHTING, WATER),
				new DualType(FLYING, ROCK),
				new DualType(FLYING, WATER),
				new DualType(FLYING, ELECTRIC),
				new DualType(FLYING, ICE),
				new DualType(FLYING, DRAGON),
				new DualType(GRASS, NO_TYPE),
				new DualType(PSYCHIC, ICE)
			};
			DUAL_TYPES_OF_GEN_2 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_1)
			{
				new DualType(STEEL, ELECTRIC),
				new DualType(NORMAL, PSYCHIC),
				new DualType(FIGHTING, BUG),
				new DualType(FLYING, GROUND),
				new DualType(FLYING, STEEL),
				new DualType(FLYING, GRASS),
				new DualType(FLYING, PSYCHIC),
				new DualType(FLYING, DARK),
				new DualType(GROUND, STEEL),
				new DualType(GROUND, WATER),
				new DualType(GROUND, ICE),
				new DualType(ROCK, NO_TYPE),
				new DualType(ROCK, BUG),
				new DualType(ROCK, FIRE),
				new DualType(ROCK, DARK),
				new DualType(GHOST, NO_TYPE),
				new DualType(FIRE, DARK),
				new DualType(WATER, ELECTRIC),
				new DualType(WATER, DRAGON),
				new DualType(ICE, DARK),
				new DualType(BUG, STEEL),
				new DualType(DARK, NO_TYPE)
			};
			DUAL_TYPES_OF_GEN_3 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_2)
			{
				new DualType(ICE, NO_TYPE),
				new DualType(ROCK, STEEL),
				new DualType(WATER, GRASS),
				new DualType(WATER, DARK),
				new DualType(GRASS, DARK),
				new DualType(FIGHTING, FIRE),
				new DualType(FIGHTING, PSYCHIC),
				new DualType(GROUND, FIRE),
				new DualType(GROUND, PSYCHIC),
				new DualType(GROUND, DRAGON),
				new DualType(ROCK, GRASS),
				new DualType(ROCK, PSYCHIC),
				new DualType(FIGHTING, GRASS),
				new DualType(GROUND, BUG),
				new DualType(BUG, GHOST),
				new DualType(BUG, WATER),
				new DualType(GHOST, DARK),
				new DualType(STEEL, NO_TYPE),
				new DualType(PSYCHIC, STEEL)
			};
			DUAL_TYPES_OF_GEN_4 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_3)
			{
				new DualType(POISON, DARK),
				new DualType(FIGHTING, POISON),
				new DualType(FLYING, GHOST),
				new DualType(GRASS, ICE),
				new DualType(NORMAL, WATER),
				new DualType(FIGHTING, STEEL),
				new DualType(GROUND, GRASS),
				new DualType(GHOST, ELECTRIC),
				new DualType(GHOST, ICE),
				new DualType(GHOST, DRAGON),
				new DualType(STEEL, FIRE),
				new DualType(STEEL, WATER),
				new DualType(STEEL, DRAGON),
				new DualType(FIRE, ELECTRIC),
				new DualType(GRASS, ELECTRIC),
				new DualType(ELECTRIC, ICE)
			};
			DUAL_TYPES_OF_GEN_5 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_4)
			{
				new DualType(GROUND, DARK),
				new DualType(GHOST, FIRE),
				new DualType(DRAGON, DARK),
				new DualType(NORMAL, GRASS),
				new DualType(FIGHTING, DARK),
				new DualType(GROUND, GHOST),
				new DualType(BUG, FIRE),
				new DualType(BUG, ELECTRIC),
				new DualType(GHOST, WATER),
				new DualType(STEEL, GRASS),
				new DualType(STEEL, DARK),
				new DualType(FIRE, PSYCHIC),
				new DualType(NORMAL, FIGHTING),
				new DualType(FIGHTING, ROCK),
				new DualType(FLYING, NO_TYPE),
				new DualType(GROUND, ELECTRIC),
				new DualType(FIRE, DRAGON),
				new DualType(ELECTRIC, DRAGON),
				new DualType(ICE, DRAGON)
			};
			DUAL_TYPES_OF_GEN_6 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_5)
			{
				new DualType(GHOST, GRASS),
				new DualType(GHOST, STEEL),
				new DualType(PSYCHIC, DARK),
				new DualType(NORMAL, FIRE),
				new DualType(NORMAL, ELECTRIC),
				new DualType(ROCK, ICE),
				new DualType(ROCK, DRAGON),
				new DualType(ROCK, FAIRY),
				new DualType(NORMAL, GROUND),
				new DualType(FIGHTING, FLYING),
				new DualType(POISON, DRAGON),
				new DualType(GHOST, PSYCHIC),
				new DualType(FIRE, WATER),
				new DualType(GRASS, DRAGON),
				new DualType(ELECTRIC, FAIRY),
				new DualType(DRAGON, FAIRY),
				new DualType(FAIRY, NO_TYPE),
				new DualType(FLYING, FAIRY),
				new DualType(GRASS, FAIRY),
				new DualType(FLYING, FAIRY),
				new DualType(NORMAL, FAIRY),
				new DualType(PSYCHIC, FAIRY),
				new DualType(FLYING, FAIRY),
				new DualType(STEEL, FAIRY),
				new DualType(WATER, FAIRY)
			};
			DUAL_TYPES_OF_GEN_7 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_6)
			{
				new DualType(ROCK, ELECTRIC),
				new DualType(BUG, FAIRY),
				new DualType(FIGHTING, DRAGON),
				new DualType(NORMAL, DARK),
				new DualType(POISON, FIRE),
				new DualType(STEEL, ICE),
				new DualType(ELECTRIC, PSYCHIC),
				new DualType(FIGHTING, GHOST),
				new DualType(FIGHTING, ICE),
				new DualType(GHOST, FAIRY),
				new DualType(ICE, FAIRY),
				new DualType(NORMAL, DRAGON),
				new DualType(POISON, ROCK)
			};
			DUAL_TYPES_OF_GEN_8 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_7)
			{
				new DualType(BUG, ICE),
				new DualType(BUG, PSYCHIC),
				new DualType(NORMAL, GHOST),
				new DualType(POISON, ELECTRIC),
				new DualType(POISON, PSYCHIC),
				new DualType(ELECTRIC, DARK),
				new DualType(FIRE, ICE),
				new DualType(POISON, FAIRY),
				new DualType(ROCK, ELECTRIC)
			};
			DUAL_TYPES_OF_GEN_9 = new SortedSet<DualType>(DUAL_TYPES_OF_GEN_8)
			{
				new DualType(BUG, DARK),
				new DualType(FIGHTING, ELECTRIC),
				new DualType(NORMAL, POISON),
				new DualType(FIRE, GRASS),
				new DualType(POISON, STEEL),
				new DualType(FIGHTING, GROUND),
				new DualType(FIGHTING, FAIRY)
			};

			TYPE_TABLE = new Dictionary<(Type, Type), float[]>();
			foreach (Type atk in (Type[])Enum.GetValues(typeof(Type)))
			{
				if (atk == NO_TYPE)
				{
					continue;
				}
				foreach (Type def in (Type[])Enum.GetValues(typeof(Type)))
				{
					if (def == NO_TYPE)
					{
						continue;
					}
					TYPE_TABLE.Add((atk, def), new float[] { 1.0f, 1.0f, 1.0f });
					if (def == STEEL || def == DARK)
					{
						TYPE_TABLE[(atk, def)][0] = -1.0f;
					}
					else if (def == FAIRY)
					{
						TYPE_TABLE[(atk, def)][0] = TYPE_TABLE[(atk, def)][1] = -1.0f;
					}
				}
			}
			//NORMAL
			TYPE_TABLE[(NORMAL, ROCK)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(NORMAL, GHOST)] = new float[] { 0.0f, 0.0f, 0.0f };
			TYPE_TABLE[(NORMAL, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			//FIGHTING
			TYPE_TABLE[(FIGHTING, NORMAL)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIGHTING, FLYING)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIGHTING, POISON)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIGHTING, ROCK)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIGHTING, BUG)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIGHTING, GHOST)] = new float[] { 0.0f, 0.0f, 0.0f };
			TYPE_TABLE[(FIGHTING, STEEL)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIGHTING, PSYCHIC)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIGHTING, ICE)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIGHTING, DARK)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIGHTING, FAIRY)] = new float[] { -1.0f, -1.0f, 0.5f };
			//FLYING
			TYPE_TABLE[(FLYING, FIGHTING)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FLYING, ROCK)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FLYING, BUG)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FLYING, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(FLYING, GRASS)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FLYING, ELECTRIC)] = new float[] { 2.0f, 2.0f, 2.0f };
			//POISON
			TYPE_TABLE[(POISON, POISON)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(POISON, GROUND)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(POISON, ROCK)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(POISON, BUG)] = new float[] { 2.0f, 1.0f, 1.0f };
			TYPE_TABLE[(POISON, GHOST)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(POISON, STEEL)] = new float[] { -1.0f, 0.0f, 0.0f };
			TYPE_TABLE[(POISON, GRASS)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(POISON, FAIRY)] = new float[] { -1.0f, -1.0f, 2.0f };
			//GROUND
			TYPE_TABLE[(GROUND, FLYING)] = new float[] { 0.0f, 0.0f, 0.0f };
			TYPE_TABLE[(GROUND, POISON)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GROUND, ROCK)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GROUND, BUG)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GROUND, STEEL)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GROUND, FIRE)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GROUND, GRASS)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GROUND, ELECTRIC)] = new float[] { 2.0f, 2.0f, 2.0f };
			//STONE
			TYPE_TABLE[(ROCK, FIGHTING)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(ROCK, FLYING)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ROCK, GROUND)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(ROCK, BUG)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ROCK, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(ROCK, FIRE)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ROCK, ICE)] = new float[] { 2.0f, 2.0f, 2.0f };
			//BUG
			TYPE_TABLE[(BUG, FIGHTING)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(BUG, FLYING)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(BUG, POISON)] = new float[] { 2.0f, 0.5f, 0.5f };
			TYPE_TABLE[(BUG, GHOST)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(BUG, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(BUG, FIRE)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(BUG, GRASS)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(BUG, PSYCHIC)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(BUG, DARK)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(BUG, FAIRY)] = new float[] { -1.0f, -1.0f, 0.5f };
			//GHOST
			TYPE_TABLE[(GHOST, NORMAL)] = new float[] { 0.0f, 0.0f, 0.0f };
			TYPE_TABLE[(GHOST, GHOST)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GHOST, STEEL)] = new float[] { -1.0f, 0.5f, 1.0f };
			TYPE_TABLE[(GHOST, PSYCHIC)] = new float[] { 0.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GHOST, DARK)] = new float[] { -1.0f, 0.5f, 0.5f };
			//STEEL
			TYPE_TABLE[(STEEL, ROCK)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(STEEL, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(STEEL, FIRE)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(STEEL, WATER)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(STEEL, ELECTRIC)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(STEEL, ICE)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(STEEL, FAIRY)] = new float[] { -1.0f, -1.0f, 2.0f };
			//FIRE
			TYPE_TABLE[(FIRE, ROCK)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIRE, BUG)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIRE, STEEL)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIRE, FIRE)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIRE, WATER)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(FIRE, GRASS)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIRE, ICE)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(FIRE, DRAGON)] = new float[] { 0.5f, 0.5f, 0.5f };
			//WATER
			TYPE_TABLE[(WATER, GROUND)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(WATER, ROCK)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(WATER, FIRE)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(WATER, WATER)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(WATER, GRASS)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(WATER, DRAGON)] = new float[] { 0.5f, 0.5f, 0.5f };
			//GRASS
			TYPE_TABLE[(GRASS, FLYING)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GRASS, POISON)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GRASS, GROUND)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GRASS, ROCK)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GRASS, BUG)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GRASS, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(GRASS, FIRE)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GRASS, WATER)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(GRASS, GRASS)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(GRASS, DRAGON)] = new float[] { 0.5f, 0.5f, 0.5f };
			//ELECTRO
			TYPE_TABLE[(ELECTRIC, FLYING)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ELECTRIC, GROUND)] = new float[] { 0.0f, 0.0f, 0.0f };
			TYPE_TABLE[(ELECTRIC, WATER)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ELECTRIC, GRASS)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(ELECTRIC, ELECTRIC)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(ELECTRIC, DRAGON)] = new float[] { 0.5f, 0.5f, 0.5f };
			//PSYCHIC
			TYPE_TABLE[(PSYCHIC, FIGHTING)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(PSYCHIC, POISON)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(PSYCHIC, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(PSYCHIC, PSYCHIC)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(PSYCHIC, DARK)] = new float[] { -1.0f, 0.0f, 0.0f };
			//ICE
			TYPE_TABLE[(ICE, FLYING)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ICE, GROUND)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ICE, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(ICE, FIRE)] = new float[] { 1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(ICE, WATER)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(ICE, GRASS)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(ICE, ICE)] = new float[] { 0.5f, 0.5f, 0.5f };
			TYPE_TABLE[(ICE, DRAGON)] = new float[] { 2.0f, 2.0f, 2.0f };
			//DRAGON
			TYPE_TABLE[(DRAGON, STEEL)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(DRAGON, DRAGON)] = new float[] { 2.0f, 2.0f, 2.0f };
			TYPE_TABLE[(DRAGON, FAIRY)] = new float[] { -1.0f, -1.0f, 0.0f };
			//DARK
			TYPE_TABLE[(DARK, FIGHTING)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(DARK, GHOST)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(DARK, STEEL)] = new float[] { -1.0f, 0.5f, 1.0f };
			TYPE_TABLE[(DARK, PSYCHIC)] = new float[] { -1.0f, 2.0f, 2.0f };
			TYPE_TABLE[(DARK, DARK)] = new float[] { -1.0f, 0.5f, 0.5f };
			TYPE_TABLE[(DARK, FAIRY)] = new float[] { -1.0f, -1.0f, 0.5f };
			//FAIRY
			TYPE_TABLE[(FAIRY, FIGHTING)] = new float[] { -1.0f, -1.0f, 2.0f };
			TYPE_TABLE[(FAIRY, POISON)] = new float[] { -1.0f, -1.0f, 0.5f };
			TYPE_TABLE[(FAIRY, STEEL)] = new float[] { -1.0f, -1.0f, 0.5f };
			TYPE_TABLE[(FAIRY, FIRE)] = new float[] { -1.0f, -1.0f, 0.5f };
			TYPE_TABLE[(FAIRY, DRAGON)] = new float[] { -1.0f, -1.0f, 2.0f };
			TYPE_TABLE[(FAIRY, DARK)] = new float[] { -1.0f, -1.0f, 2.0f };
		}
	}
}