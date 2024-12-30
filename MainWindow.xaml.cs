using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static coverage_checker.Type; 
using DualType = System.Tuple<coverage_checker.Type, coverage_checker.Type>;

namespace coverage_checker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
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
		private bool showActual = true, ninjatom = false;
		int immunities, weaknesses, neutrals, strengths, points, numTypes, dualImmunities, dualWeaknesses, 
			dualNeutrals, dualStrengths, dualPoints, dualNumTypes,
			dualDoubleStrengths, dualDoubleWeaknesses, tier, dualTier;
		private bool firstActual = true, firstSelection = true;

		public MainWindow()
		{
			InitializeComponent();
			checkBoxes = new CheckBox[] { fire_cb, water_cb, ghost_cb, fly_cb, dragon_cb,
				bug_cb, grass_cb, ground_cb, stone_cb, dark_cb, electric_cb, ice_cb, poison_cb,
				fairy_cb, steel_cb, fight_cb, normal_cb, psychic_cb};
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
			dualTypes = TypeMethods.ActualDualTypesOfGen(gen);
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
					float factor = TypeMethods.TypeFactor(atk, def, gen);
					if (factor == maxFactor)
						types.Add(atk);
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
						immuneTypes.Add(def.Item1);
				}
				else if (maxFactor == 0.25f)
				{
					dualDoubleWeakTypes.Add(def, types);
				}
				else if (maxFactor == 0.5f)
				{
					dualWeakTypes.Add(def, types);
					if (def.Item2 == NO_TYPE)
						weakTypes.Add(def.Item1, types);
				}
				else if (maxFactor == 1.0f)
				{
					dualNeutralTypes.Add(def, types);
					if (def.Item2 == NO_TYPE)
						neutralTypes.Add(def.Item1, types);
				}
				else if (maxFactor == 2.0f)
				{
					dualStrongTypes.Add(def, types);
					if (def.Item2 == NO_TYPE)
						strongTypes.Add(def.Item1, types);
				}
				else
				{
					dualDoubleStrongTypes.Add(def, types);
				}
			}

			immunities = immuneTypes.Count;
			weaknesses = weakTypes.Keys.Count;
			neutrals = neutralTypes.Keys.Count;
			strengths = strongTypes.Keys.Count;

			points = 4 * strengths + 2 * neutrals - weaknesses - 10 * immunities;
			numTypes = gen == 1 ? 15 : gen < 6 ? 17 : 18;

			if (immunities == 0)
				if (weaknesses == 0)
					if (neutrals == 0)
						tier = 3;
					else
						tier = 2;
				else
					tier = 1;
			else
				tier = 0;

			dualImmunities = dualImmuneTypes.Count;
			dualWeaknesses = dualWeakTypes.Keys.Count;
			dualNeutrals = dualNeutralTypes.Keys.Count;
			dualStrengths = dualStrongTypes.Keys.Count;
			dualDoubleStrengths = dualDoubleStrongTypes.Keys.Count;
			dualDoubleWeaknesses = dualDoubleWeakTypes.Keys.Count;

			dualPoints = 8 * dualDoubleStrengths + 4 * dualStrengths + 2 * dualNeutrals
				- dualWeaknesses - 2 * dualDoubleWeaknesses - 10 * dualImmunities;
			dualNumTypes = dualTypes.Count;

			if (dualImmunities == 0)
				if (dualDoubleWeaknesses == 0)
					if (dualWeaknesses == 0)
						if (dualNeutrals == 0)
							dualTier = 4;
						else
							dualTier = 3;
					else
						dualTier = 2;
				else
					dualTier = 1;
			else
				dualTier = 0;
		}

		private void Write()
		{
			//Print Single Texts
			strongTextBox.Text = GenerateText(strongTypes, "Du hast sehr Effektive Attacken gegen:");
			neutralTextBox.Text = GenerateText(neutralTypes, "Du hast immerhin neutrale Attacken gegen:");
			weakTextBox.Text = GenerateText(weakTypes, "Du hast nur nicht sehr Effektive Attacken gegen:");
			immuneTextBox.Text = GenerateText(immuneTypes, "Du hast keine Attacken gegen:", true);

			//Points
			pointsTextBox.Text = points + "/" + 4 * numTypes;

			//Verdict
			verdictTextBox.Text = tier switch
			{
				0 => "TIER 0 Coverage. Es gibt Typen, die du nicht bekämpfen kannst!",
				1 => "TIER 1 Coverage. Du kannst jeden Typen bekämpfen.",
				2 => "TIER 2 Coverage! Du triffst jeden Typen mindestens neutral.",
				3 => "TIER 3 Coverage! Du triffst jeden Typen sehr Effektiv!",
				_ => "ERROR",
			};

			//Print Dual Texts
			dualStrongTextBox.Text = GenerateDualText(dualStrongTypes, "Du hast sehr Effektive Attacken gegen:");
			dualDoubleStrongTextBox.Text = GenerateDualText(dualDoubleStrongTypes, "Du hast super Effektive Attacken gegen:");
			dualNeutralTextBox.Text = GenerateDualText(dualNeutralTypes, "Du hast immerhin neutrale Attacken gegen:");
			dualWeakTextBox.Text = GenerateDualText(dualWeakTypes, "Du hast nur nicht sehr Effektive Attacken gegen:");
			dualDoubleWeakTextBox.Text = GenerateDualText(dualDoubleWeakTypes, "Du hast nur doppelt nicht sehr Effektive Attacken gegen:");
			dualImmuneTextBox.Text = GenerateDualText(dualImmuneTypes, "Du hast keine Attacken gegen:", true);

			//Points
			dualPointsTextBox.Text = dualPoints + "/" + 8 * dualNumTypes;

			//Verdict
			dualVerdictTextBox.Text = dualTier switch
			{
				0 => "TIER 0 Coverage. Es gibt Typen, die du nicht bekämpfen kannst!",
				1 => "TIER 1 Coverage. Du kannst jeden Typen gerade bekämpfen.",
				2 => "TIER 2 Coverage. Du kannst jeden Typen bekämpfen.",
				3 => "TIER 3 Coverage! Du triffst jeden Typen mindestens neutral.",
				4 => "TIER 4 Coverage! Du triffst jeden Typen sehr Effektiv!",
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
				s.Append(" ");
				List<DualType> list = (List<DualType>)col;
				if (list.Count == 0)
					s.Append("nichts!  ");
				foreach (DualType t in list)
				{
					s.Append(TypeMethods.DualTypeToString(t)).Append(", ");
				}
				//Ninjatom
				if (ninjatom && gen >= 3 && !HasNinjatomCoveradge(selectedTypes))
				{
					s = new StringBuilder(s.ToString().Trim());
					s.Append(list.Count == 0 ? " außer Ninjatom" : " Ninjatom");
				}
				else
					s.Remove(s.Length - 2, 2);
			}
			else
			{
				s.Append("\n");
				Dictionary<DualType, List<Type>> dict = (Dictionary<DualType, List<Type>>)col;
				if (dict.Keys.Count == 0)
					s.Append("    nichts!");
				foreach (DualType t in dict.Keys)
				{
					s.Append("    ").Append(TypeMethods.DualTypeToString(t)).Append(" mit ");
					foreach (Type t2 in dict[t])
					{
						s.Append(TypeMethods.TypeToString(t2)).Append(", ");
					}
					s.Remove(s.Length - 2, 2).Append("\n");
				}
			}
			return s.ToString();
		}

		private string GenerateText(ICollection col, string startText, bool immune = false)
		{
			StringBuilder s = new StringBuilder(startText);
			if (immune)
			{
				s.Append(" ");
				List<Type> list = (List<Type>)col;
				if (list.Count == 0)
					s.Append("nichts!  ");
				foreach (Type t in list)
				{
					s.Append(TypeMethods.TypeToString(t)).Append(", ");
				}
				//Ninjatom
				if (ninjatom && gen >= 3 && !HasNinjatomCoveradge(selectedTypes))
				{
					s = new StringBuilder(s.ToString().Trim());
					s.Append(list.Count == 0 ? " außer Ninjatom" : " Ninjatom");
				}
				else
					s.Remove(s.Length - 2, 2);
			}
			else
			{
				s.Append("\n");
				Dictionary<Type, List<Type>> dict = (Dictionary<Type, List<Type>>)col;
				if (dict.Keys.Count == 0)
					s.Append("    nichts!");
				foreach (Type t in dict.Keys)
				{
					s.Append("    ").Append(TypeMethods.TypeToString(t)).Append(" mit ");
					foreach (Type t2 in dict[t])
					{
						s.Append(TypeMethods.TypeToString(t2)).Append(", ");
					}
					s.Remove(s.Length - 2, 2).Append("\n");
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
				cb.IsChecked = false;
			foreach (TextBox tb in textBoxes)
				tb.Text = "";
		}

		private void Cb_Checked(object sender, RoutedEventArgs e)
		{
			selectedTypes.Add(TypeMethods.StringToType((string)((CheckBox)sender).Content));
		}

		private void Cb_Unchecked(object sender, RoutedEventArgs e)
		{
			selectedTypes.Remove(TypeMethods.StringToType((string)((CheckBox)sender).Content));
		}

		private void Ninjatom_Checked(object sender, RoutedEventArgs e)
		{
			ninjatom = true;
			//Update();
			//Write();
		}

		private void Ninjatom_Unchecked(object sender, RoutedEventArgs e)
		{
			ninjatom = false;
			//Update();
			//Write();
		}

		private void Actual_Checked(object sender, RoutedEventArgs e)
		{
			if (firstActual)
			{
				firstActual = false;
				return;
			}
			showActual = true;
			dualTypes = TypeMethods.ActualDualTypesOfGen(gen);
			//Update();
			//Write();
		}

		private void Actual_Unchecked(object sender, RoutedEventArgs e)
		{
			showActual = false;
			dualTypes = TypeMethods.DualTypesOfGen(gen);
			//Update();
			//Write();
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
				steel_cb.IsEnabled = dark_cb.IsEnabled = true;
			if (gen > 5)
				fairy_cb.IsEnabled = true;
			dualTypes = showActual ? TypeMethods.ActualDualTypesOfGen(gen) : TypeMethods.DualTypesOfGen(gen);
		}

		private void Find_Optimal(object sender, RoutedEventArgs e)
		{
			bool isMin = rb_min.IsChecked.Value;
			int n = int.Parse(tb_n.Text);
			if (isMin)
				FindMinMovesWithTier(n);
			else
				FindMaxScoreWithMoves(n);
			Reset();
		}

		private void FindMaxScoreWithMoves(int n)
		{
			SortedList<int, List<Type>> result = new SortedList<int, List<Type>>(new IteratorHelper.DescendingDuplicateKeyComparer<int>());
			int optimalTier = 0;
			foreach (List<Type> types in IteratorHelper.Subsets(TypeMethods.TypesOfGen(gen).ToList(), n))
			{
				selectedTypes = types;
				Update();
				if (dualTier == optimalTier)
					result.Add(dualPoints, types);
				else if (dualTier > optimalTier)
				{
					optimalTier = dualTier;
					result.Clear();
					result.Add(dualPoints, types);
				}
			}
			StringBuilder s = new StringBuilder("Optimale Tier ");
			s.Append(optimalTier).Append(" Coveradge mit ").Append(n).Append(" Typen: \n");
			foreach ((int points, List<Type> types) in result)
			{
				//Ninjatom clause
				if (ninjatom && !HasNinjatomCoveradge(types))
					continue;
				s.Append("   ");
				foreach (Type t in types)
				{
					s.Append(TypeMethods.TypeToString(t)).Append(", ");
				}
				s.Remove(s.Length - 2, 2).Append(": Punkte: ").Append(points).Append("\n");
			}
			optimumTextBox.Text = s.ToString();
		}

		private void FindMinMovesWithTier(int n)
		{
			SortedList<int, List<Type>> result = new SortedList<int, List<Type>>(new IteratorHelper.DescendingDuplicateKeyComparer<int>());
			int optimalTier = 0;
			int maxMoves = 0;
			while(optimalTier < n)
			{
				++maxMoves;
				foreach (List<Type> types in IteratorHelper.Subsets(TypeMethods.TypesOfGen(gen).ToList(), maxMoves))
				{
					if (types.Count < maxMoves)
						continue;
					selectedTypes = types;
					Update();
					if (dualTier == optimalTier)
						result.Add(dualPoints, types);
					else if (dualTier > optimalTier)
					{
						optimalTier = dualTier;
						result.Clear();
						result.Add(dualPoints, types);
					}
				}
			}
			StringBuilder s = new StringBuilder("Tier "); 
			s.Append(n).Append(" ist möglich mit ").Append(maxMoves).Append(" Typen: \n");
			foreach ((_, List<Type> types) in result)
			{
				//Ninjatom clause
				if (ninjatom && !HasNinjatomCoveradge(types))
					continue;
				s.Append("   ");
				foreach (Type t in types)
				{
					s.Append(TypeMethods.TypeToString(t)).Append(", ");
				}
				s.Remove(s.Length - 2, 2).Append("\n");
			}
			optimumTextBox.Text = s.ToString();
		}

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private bool HasNinjatomCoveradge(List<Type> types)
		{
			return types.Contains(FIRE) || types.Contains(FLYING) || types.Contains(ROCK) || types.Contains(GHOST) || types.Contains(DARK);
		}
	}
}