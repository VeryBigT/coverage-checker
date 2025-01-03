using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static coverage_checker.Type;
using static coverage_checker.StringHelper;
using static coverage_checker.IteratorHelper;
using static coverage_checker.TypeMethods;
using DualType = System.Tuple<coverage_checker.Type, coverage_checker.Type>;

namespace coverage_checker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly CheckBox[] checkBoxes;
		private readonly TextBox[] textBoxes;
		private List<Type> selectedTypes;
		private readonly Dictionary<Type, List<Type>> strongTypes;
		private readonly Dictionary<Type, List<Type>> neutralTypes;
		private readonly Dictionary<Type, List<Type>> weakTypes;
		private readonly List<Type> immuneTypes;

		private readonly Dictionary<DualType, List<Type>> dualStrongTypes;
		private readonly Dictionary<DualType, List<Type>> dualDoubleStrongTypes;
		private readonly Dictionary<DualType, List<Type>> dualNeutralTypes;
		private readonly Dictionary<DualType, List<Type>> dualWeakTypes;
		private readonly Dictionary<DualType, List<Type>> dualDoubleWeakTypes;
		private readonly List<DualType> dualImmuneTypes;
		private ISet<DualType> dualTypes;

		private int gen;
		private bool showActual = true, shedinja = false;
		private int immunities, weaknesses, neutrals, strengths, points, numTypes, dualImmunities, dualWeaknesses,
			dualNeutrals, dualStrengths, dualPoints, dualNumTypes,
			dualDoubleStrengths, dualDoubleWeaknesses, tier, dualTier;
		private bool firstSelection = true;

		public MainWindow()
		{
			InitializeComponent();
			checkBoxes = new CheckBox[] { fire_cb, water_cb, ghost_cb, flying_cb, dragon_cb,
				bug_cb, grass_cb, ground_cb, rock_cb, dark_cb, electric_cb, ice_cb, poison_cb,
				fairy_cb, steel_cb, fighting_cb, normal_cb, psychic_cb};
			textBoxes = new TextBox[] { strongTextBox, neutralTextBox, weakTextBox, immuneTextBox,
				verdictTextBox, pointsTextBox, dualStrongTextBox, dualDoubleStrongTextBox, dualNeutralTextBox,
				dualWeakTextBox, dualDoubleWeakTextBox, dualImmuneTextBox, dualVerdictTextBox, dualPointsTextBox};
			selectedTypes = new List<Type>();
			strongTypes = new Dictionary<Type, List<Type>>();
			neutralTypes = new Dictionary<Type, List<Type>>();
			weakTypes = new Dictionary<Type, List<Type>>();
			immuneTypes = new List<Type>();
			dualStrongTypes = new Dictionary<DualType, List<Type>>();
			dualDoubleStrongTypes = new Dictionary<DualType, List<Type>>();
			dualNeutralTypes = new Dictionary<DualType, List<Type>>();
			dualWeakTypes = new Dictionary<DualType, List<Type>>();
			dualDoubleWeakTypes = new Dictionary<DualType, List<Type>>();
			dualImmuneTypes = new List<DualType>();
			gen = 3;
			dualTypes = ActualDualTypesOfGen(gen);
		}

		private void Update()
		{
			immuneTypes.Clear();
			weakTypes.Clear();
			neutralTypes.Clear();
			strongTypes.Clear();

			dualImmuneTypes.Clear();
			dualWeakTypes.Clear();
			dualDoubleWeakTypes.Clear();
			dualNeutralTypes.Clear();
			dualStrongTypes.Clear();
			dualDoubleStrongTypes.Clear();

			List<Type> types;
			foreach (DualType def in dualTypes)
			{
				float maxFactor = 0.0f;
				types = new List<Type>();
				foreach (Type atk in selectedTypes)
				{
					float factor = TypeFactor(atk, def, gen);
					if (factor == maxFactor)
					{
						types.Add(atk);
					}
					else if (factor > maxFactor)
					{
						maxFactor = factor;
						types.Clear();
						types.Add(atk);
					}
				}
				if (maxFactor == 0.0f)
				{
					dualImmuneTypes.Add(def);
					if (def.Item2 == NO_TYPE)
					{
						immuneTypes.Add(def.Item1);
					}
				}
				else if (maxFactor == 0.25f)
				{
					dualDoubleWeakTypes.Add(def, types);
				}
				else if (maxFactor == 0.5f)
				{
					dualWeakTypes.Add(def, types);
					if (def.Item2 == NO_TYPE)
					{
						weakTypes.Add(def.Item1, types);
					}
				}
				else if (maxFactor == 1.0f)
				{
					dualNeutralTypes.Add(def, types);
					if (def.Item2 == NO_TYPE)
					{
						neutralTypes.Add(def.Item1, types);
					}
				}
				else if (maxFactor == 2.0f)
				{
					dualStrongTypes.Add(def, types);
					if (def.Item2 == NO_TYPE)
					{
						strongTypes.Add(def.Item1, types);
					}
				}
				else //if (maxFactor == 4.0f)
				{
					dualDoubleStrongTypes.Add(def, types);
				}
			}

			immunities = immuneTypes.Count;
			weaknesses = weakTypes.Keys.Count;
			neutrals = neutralTypes.Keys.Count;
			strengths = strongTypes.Keys.Count;

			points = (4 * strengths) + (2 * neutrals) - weaknesses - (10 * immunities);
			numTypes = gen == 1 ? 15 : gen < 6 ? 17 : 18;

			tier = immunities == 0 ? weaknesses == 0 ? neutrals == 0 ? 3 : 2 : 1 : 0;

			dualImmunities = dualImmuneTypes.Count;
			dualWeaknesses = dualWeakTypes.Keys.Count;
			dualNeutrals = dualNeutralTypes.Keys.Count;
			dualStrengths = dualStrongTypes.Keys.Count;
			dualDoubleStrengths = dualDoubleStrongTypes.Keys.Count;
			dualDoubleWeaknesses = dualDoubleWeakTypes.Keys.Count;

			dualPoints = (8 * dualDoubleStrengths) + (4 * dualStrengths) + (2 * dualNeutrals)
				- dualWeaknesses - (2 * dualDoubleWeaknesses) - (10 * dualImmunities);
			dualNumTypes = dualTypes.Count;

			dualTier = dualImmunities == 0 ? dualDoubleWeaknesses == 0 ? dualWeaknesses == 0
				? dualNeutrals == 0 ? 4 : 3 : 2 : 1 : 0;
		}

		private void Write()
		{
			//Print Single Texts
			strongTextBox.Text = GenerateText(strongTypes, GetString("strength_text"));
			neutralTextBox.Text = GenerateText(neutralTypes, GetString("neutral_text"));
			weakTextBox.Text = GenerateText(weakTypes, GetString("weakness_text"));
			immuneTextBox.Text = GenerateText(immuneTypes, GetString("immune_text"), true);

			//Points
			pointsTextBox.Text = points + "/" + (4 * numTypes);

			//Verdict
			verdictTextBox.Text = tier switch
			{
				0 => GetString("tier_0_verdict"),
				1 => GetString("tier_2_verdict"),
				2 => GetString("tier_3_verdict"),
				3 => GetString("tier_4_verdict"),
				_ => "ERROR",
			};

			//Print Dual Texts
			dualStrongTextBox.Text = GenerateDualText(dualStrongTypes, GetString("strength_text"));
			dualDoubleStrongTextBox.Text = GenerateDualText(dualDoubleStrongTypes, GetString("double_strength_text"));
			dualNeutralTextBox.Text = GenerateDualText(dualNeutralTypes, GetString("neutral_text"));
			dualWeakTextBox.Text = GenerateDualText(dualWeakTypes, GetString("weakness_text"));
			dualDoubleWeakTextBox.Text = GenerateDualText(dualDoubleWeakTypes, GetString("double_weakness_text"));
			dualImmuneTextBox.Text = GenerateDualText(dualImmuneTypes, GetString("immune_text"), true);

			//Points
			dualPointsTextBox.Text = dualPoints + "/" + (8 * dualNumTypes);

			//Verdict
			dualVerdictTextBox.Text = dualTier switch
			{
				0 => GetString("tier_0_verdict"),
				1 => GetString("tier_1_verdict"),
				2 => GetString("tier_2_verdict"),
				3 => GetString("tier_3_verdict"),
				4 => GetString("tier_4_verdict"),
				_ => "ERROR",
			};
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{
			Update();
			Write();
		}

		private string GenerateDualText(ICollection col, string startText, bool immune = false)
		{
			StringBuilder s = new StringBuilder(startText);
			if (immune)
			{
				_ = s.Append(" ");
				List<DualType> list = (List<DualType>)col;
				if (list.Count == 0)
				{
					_ = s.Append(GetString("nothing")).Append("!  ");
				}
				foreach (DualType t in list)
				{
					_ = s.Append(DualTypeToString(t)).Append(", ");
				}
				//shedinja
				if (shedinja && gen >= 3 && !HasShedinjaCoverage(selectedTypes))
				{
					s = new StringBuilder(s.ToString().Trim());
					_ = s.Append(" ").Append(list.Count == 0
						? GetString("except") + " " + GetString("shedinja")
						: GetString("shedinja"));
				}
				else
				{
					_ = s.Remove(s.Length - 2, 2);
				}
			}
			else
			{
				_ = s.Append("\n");
				Dictionary<DualType, List<Type>> dict = (Dictionary<DualType, List<Type>>)col;
				if (dict.Keys.Count == 0)
				{
					_ = s.Append("    ").Append(GetString("nothing")).Append("!");
				}
				foreach (DualType t in dict.Keys)
				{
					_ = s.Append("    ").Append(DualTypeToString(t)).Append(" ")
						.Append(GetString("with")).Append(" ");
					foreach (Type t2 in dict[t])
					{
						_ = s.Append(TypeToString(t2)).Append(", ");
					}
					_ = s.Remove(s.Length - 2, 2).Append("\n");
				}
			}
			return s.ToString();
		}

		private string GenerateText(ICollection col, string startText, bool immune = false)
		{
			StringBuilder s = new StringBuilder(startText);
			if (immune)
			{
				_ = s.Append(" ");
				List<Type> list = (List<Type>)col;
				if (list.Count == 0)
				{
					_ = s.Append(GetString("nothing")).Append("!  ");
				}
				foreach (Type t in list)
				{
					_ = s.Append(TypeToString(t)).Append(", ");
				}
				//shedinja
				if (shedinja && gen >= 3 && !HasShedinjaCoverage(selectedTypes))
				{
					s = new StringBuilder(s.ToString().Trim());
					_ = s.Append(" ").Append(list.Count == 0
						? GetString("except") + " " + GetString("shedinja")
						: GetString("shedinja"));
				}
				else
				{
					_ = s.Remove(s.Length - 2, 2);
				}
			}
			else
			{
				_ = s.Append("\n");
				Dictionary<Type, List<Type>> dict = (Dictionary<Type, List<Type>>)col;
				if (dict.Keys.Count == 0)
				{
					_ = s.Append("    ").Append(GetString("nothing")).Append("!");
				}
				foreach (Type t in dict.Keys)
				{
					_ = s.Append("    ").Append(TypeToString(t)).Append(" ")
						.Append(GetString("with")).Append(" ");
					foreach (Type t2 in dict[t])
					{
						_ = s.Append(TypeToString(t2)).Append(", ");
					}
					_ = s.Remove(s.Length - 2, 2).Append("\n");
				}
			}
			return s.ToString();
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Reset();
		}

		private void Reset()
		{
			foreach (CheckBox cb in checkBoxes)
			{
				cb.IsChecked = false;
			}
			foreach (TextBox tb in textBoxes)
			{
				tb.Text = "";
			}
		}

		private void Cb_Checked(object sender, RoutedEventArgs e)
		{
			selectedTypes.Add(StringToType(((CheckBox)sender).Name[0..^3]));
		}

		private void Cb_Unchecked(object sender, RoutedEventArgs e)
		{
			_ = selectedTypes.Remove(StringToType(((CheckBox)sender).Name[0..^3]));
		}

		private void Ninjatom_Checked(object sender, RoutedEventArgs e)
		{
			shedinja = true;
		}

		private void Ninjatom_Unchecked(object sender, RoutedEventArgs e)
		{
			shedinja = false;
		}

		private void Actual_Checked(object sender, RoutedEventArgs e)
		{
			showActual = true;
			dualTypes = ActualDualTypesOfGen(gen);
		}

		private void Actual_Unchecked(object sender, RoutedEventArgs e)
		{
			showActual = false;
			dualTypes = DualTypesOfGen(gen);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (firstSelection)
			{
				firstSelection = false;
				return;
			}
			gen = ((ComboBox)sender).SelectedIndex + 1;
			steel_cb.IsEnabled = dark_cb.IsEnabled = fairy_cb.IsEnabled = false;
			if (gen > 1)
			{
				steel_cb.IsEnabled = dark_cb.IsEnabled = true;
			}
			if (gen > 5)
			{
				fairy_cb.IsEnabled = true;
			}
			dualTypes = showActual ? ActualDualTypesOfGen(gen) : DualTypesOfGen(gen);
		}

		private void ComboBox_LanguageChanged(object sender, SelectionChangedEventArgs e)
		{
			SetLanguage(((ComboBoxItem)((ComboBox)sender).SelectedItem).Name);
		}

		private void SetLanguage(string language)
		{
			ResourceDictionary dict = new ResourceDictionary
			{
				Source = new Uri("Resources\\Languages\\StringResources." + language + ".xaml", UriKind.Relative)
			};
			Application.Current.Resources.MergedDictionaries.Add(dict);
		}

		private void Find_Optimal(object sender, RoutedEventArgs e)
		{
			bool isMin = rb_min.IsChecked.Value;
			int n = int.Parse(tb_n.Text);
			if (isMin)
			{
				FindMinMovesWithTier(n);
			}
			else
			{
				FindMaxScoreWithMoves(n);
			}
			Reset();
		}

		private void FindMaxScoreWithMoves(int numMoves)
		{
			SortedList<int, List<Type>> result = new SortedList<int, List<Type>>(new DescendingDuplicateKeyComparer<int>());
			int optimalTier = 0;
			foreach (List<Type> types in Subsets(TypesOfGen(gen).ToList(), numMoves))
			{
				selectedTypes = types;
				Update();
				if (dualTier == optimalTier)
				{
					result.Add(dualPoints, types);
				}
				else if (dualTier > optimalTier)
				{
					optimalTier = dualTier;
					result.Clear();
					result.Add(dualPoints, types);
				}
			}
			StringBuilder s = new StringBuilder(GetFormatString("max_optimal_format", optimalTier, numMoves));
			_ = s.Append(" \n");
			foreach ((int points, List<Type> types) in result)
			{
				//shedinja clause
				if (shedinja && gen >= 3 && !HasShedinjaCoverage(types))
				{
					continue;
				}
				_ = s.Append("   ");
				foreach (Type t in types)
				{
					_ = s.Append(TypeToString(t)).Append(", ");
				}
				_ = s.Remove(s.Length - 2, 2).Append(": ").Append(GetString("Points"))
					.Append(": ").Append(points).Append("\n");
			}
			optimumTextBox.Text = s.ToString();
		}

		private void FindMinMovesWithTier(int tier)
		{
			if (tier > 4)
			{
				optimumTextBox.Text = GetFormatString("no_such_tier_format", tier);
				return;
			}
			if (tier == 4 && gen >= 3 && gen < 6)
			{
				optimumTextBox.Text = GetFormatString("no_such_tier_in_gen_format", tier, gen);
				return;
			}
			SortedList<int, List<Type>> result = new SortedList<int, List<Type>>(new DescendingDuplicateKeyComparer<int>());
			int optimalTier = 0;
			int numMoves = 0;
			while (optimalTier < tier)
			{
				++numMoves;
				foreach (List<Type> types in Subsets(TypesOfGen(gen).ToList(), numMoves))
				{
					if (types.Count < numMoves)
					{
						continue;
					}
					selectedTypes = types;
					Update();
					if (dualTier == optimalTier)
					{
						result.Add(dualPoints, types);
					}
					else if (dualTier > optimalTier)
					{
						optimalTier = dualTier;
						result.Clear();
						result.Add(dualPoints, types);
					}
				}
			}
			StringBuilder s = new StringBuilder(GetFormatString("min_optimal_format", tier, numMoves));
			_ = s.Append(" \n");
			foreach ((_, List<Type> types) in result)
			{
				//shedinja clause
				if (shedinja && gen >= 3 && !HasShedinjaCoverage(types))
				{
					continue;
				}
				_ = s.Append("   ");
				foreach (Type t in types)
				{
					_ = s.Append(TypeToString(t)).Append(", ");
				}
				_ = s.Remove(s.Length - 2, 2).Append("\n");
			}
			optimumTextBox.Text = s.ToString();
		}

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}
	}
}