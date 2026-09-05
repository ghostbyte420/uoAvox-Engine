using Server.Engines.Publishing;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
	public class DynamicBookshelf : BaseAddonContainer
	{
		private static readonly ContainerData _ContainerData = new(77, new(76, 12, 64, 56), 66);

		private static int GetFacingID(Facing facing)
		{
			switch (facing)
			{
				case Facing.East: return 0xA8E3;
				case Facing.South: return 0xA8E3;
				default: return 0xA8E3;
			}
		}

		private static int GetFacingID(Facing facing, int index)
		{
			return GetFacingID(facing) + 1 + index;
		}

		private Dictionary<Mobile, ShelfUI> _Gumps;

		public override bool HandlesOnMovement => Parent == null;

		public override bool ShareHue => false;

		public override string DefaultName => "book shelf";

		public override double DefaultWeight => 10;

		private Facing _FacingDirection = Facing.South;

		[CommandProperty(AccessLevel.GameMaster)]
		public Facing FacingDirection
		{
			get => _FacingDirection;
			set
			{
				if (_FacingDirection == value)
				{
					return;
				}

				_FacingDirection = value;

				UpdateDisplays();
			}
		}

		[Constructable]
		public DynamicBookshelf()
			: this(Facing.South)
		{ }

		[Constructable]
		public DynamicBookshelf(Facing facing)
			: base(GetFacingID(facing))
		{
			_FacingDirection = facing;

			UpdateContainerData();

			for (var i = 0; i < 7; i++)
			{
				AddComponent(new BookDisplay(i), 0, 0, 1);
			}

			for (var i = 0; i < 7; i++)
			{
				DropItem(Loot.Construct(Utility.RandomList(typeof(TanBook), typeof(RedBook), typeof(BlueBook), typeof(BrownBook))));
			}
		}

		public DynamicBookshelf(Serial serial) 
			: base(serial)
		{
			UpdateContainerData();
		}

		private IEnumerable<BookDisplay> EnumerateDisplays()
		{
			return Components.OfType<BookDisplay>().OrderBy(d => d.Index);
		}

		private BookDisplay GetDisplayAt(int x)
		{
			foreach (var display in Components.OfType<BookDisplay>())
			{
				if (display.Contains(x))
				{
					return display;
				}
			}

			return null;
		}

		public override void UpdateContainerData()
		{
			ContainerData = _ContainerData;
		}

		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			if (item is not BaseBook)
			{
				if (message)
				{
					m?.SendMessage("You may only store books here.");
				}

				return false;
			}

			return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
		}

		public override void OnItemAdded(Item item)
		{
			base.OnItemAdded(item);

			if (item is BaseBook book)
			{
				foreach (var display in EnumerateDisplays())
				{
					if (display.Contains(book.X))
					{
						display.Book = book;
						break;
					}
				}
			}
		}

		public override void OnItemRemoved(Item item)
		{
			base.OnItemRemoved(item);

			if (item is BaseBook book)
			{
				foreach (var display in EnumerateDisplays())
				{
					if (display.Book == book)
					{
						display.Book = FindItemByType<BaseBook>(b => b != book && GetDisplayAt(b.X) == display);
						break;
					}
				}
			}
		}

		public override void OnLocationChange(Point3D oldLoc)
		{
			base.OnLocationChange(oldLoc);

			UpdateDisplays();
		}

		public override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			UpdateDisplays();
		}

		public void UpdateDisplays()
		{
			foreach (var display in EnumerateDisplays())
			{
				display.UpdateDisplay(false);
			}

			if (_Gumps?.Count > 0)
			{
				Queue<ShelfUI> updates = null;

				foreach (var ui in _Gumps.Values)
				{
					if (ui.Open && !ui.SuspendLayout)
					{
						if (updates == null)
						{
							updates = new Queue<ShelfUI>();
						}

						updates.Enqueue(ui);
					}
				}

				while (updates?.Count > 0)
				{
					var ui = updates.Dequeue();

					ui.Refresh();
				}

				updates?.TrimExcess();
			}
		}

		public override void DisplayTo(Mobile to)
		{
			ProcessOpeners(to);

			if (!IsPublicContainer)
			{
				if (_Gumps?.Count > 0)
				{
					var worldLoc = GetWorldLocation();
					var map = Map;

					Queue<ShelfUI> updates = null;

					foreach (var ui in _Gumps.Values)
					{
						var range = GetUpdateRange(ui.User);

						if (ui.User.Map != map || !ui.User.InRange(worldLoc, range))
						{
							if (updates == null)
							{
								updates = new Queue<ShelfUI>();
							}

							updates.Enqueue(ui);
						}
					}

					while (updates?.Count > 0)
					{
						var ui = updates.Dequeue();

						ui.Close();
					}

					updates?.TrimExcess();
				}
			}

			if (to is PlayerMobile user)
			{
				if (user.ViewOPL)
				{
					var items = Items;

					for (var i = 0; i < items.Count; ++i)
					{
						user.Send(items[i].OPLPacket);
					}
				}

				user.SendGump(new ShelfUI(user, this));
			}
		}

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			base.OnMovement(m, oldLocation);

			if (m.Player && !m.InRange(Location, 2) && _Gumps?.TryGetValue(m, out var ui) == true)
			{
				ui?.Close();
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);

			writer.Write(FacingDirection);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt();

			FacingDirection = reader.ReadEnum<Facing>();

			Components.Sort((l, r) =>
			{
				if (l is BookDisplay dl && r is BookDisplay dr)
				{
					return dl.Index.CompareTo(dr.Index);
				}

				return l.Serial.CompareTo(r.Serial);
			});
		}

		public enum Facing : byte
		{
			East = Direction.East,
			South = Direction.South,
		}

		public sealed class BookDisplay : AddonContainerComponent
		{
			[CommandProperty(AccessLevel.GameMaster, true)]
			public int Index { get; private set; }

			public Rectangle2D Bounds { get; }

			private BaseBook _Book;

			[CommandProperty(AccessLevel.GameMaster)]
			public BaseBook Book
			{
				get => _Book;
				set
				{
					if (value?.Deleted == true)
					{
						value = null;
					}

					if (_Book == value)
					{
						return;
					}

					_Book = value;

					UpdateDisplay(true);
				}
			}

			[Hue, CommandProperty(AccessLevel.GameMaster)]
			public override int Hue
			{
				get
				{
					var hue = base.Hue;

					if (_Book?.Deleted == false)
					{
						hue = _Book.Hue;

						if (hue <= 0)
						{
							if (_Book is BlueBook)
							{
								hue = 2597;
							}
							else if (_Book is BrownBook)
							{
								hue = 1123;
							}
							else if (_Book is RedBook)
							{
								hue = 2636;
							}
							else if (_Book is TanBook)
							{
								hue = 2970;
							}
						}
					}

					return hue;
				}
				set
				{
					base.Hue = value;

					if (_Book?.Deleted == false)
					{
						_Book.Hue = value;
					}
				}
			}

			public int GumpImage => ItemID <= 1 ? 0 : (496 + (ItemID % 4));

			public override string DefaultName => _Book?.Name ?? String.Empty;

			public override double DefaultWeight => _Book?.Weight ?? 0.0;

			public BookDisplay(int index)
				: base(1)
			{
				Index = index;

				var bounds = _ContainerData.Bounds;

				var colW = bounds.Width / 7;
				var colX = bounds.X + (colW * Index);

				bounds.Set(colX, bounds.Y, colW, bounds.Height);

				Bounds = bounds;
			}

			public BookDisplay(Serial serial)
				: base(serial)
			{
			}

			public void UpdateDisplay(bool cascade)
			{
				if (Addon is DynamicBookshelf shelf)
				{
					if (_Book?.Deleted == false)
					{
						ItemID = GetFacingID(shelf.FacingDirection, Index);
					}
					else
					{
						ItemID = 1;
					}

					Delta(ItemDelta.Update);
					ReleaseWorldPackets();
					ProcessDelta();

					if (cascade)
					{
						foreach (var display in shelf.EnumerateDisplays())
						{
							if (display.Index > Index)
							{
								display.UpdateDisplay(false);
							}
						}
					}
				}
				else
				{
					ItemID = 1;

					Delta(ItemDelta.Update);
					ReleaseWorldPackets();
					ProcessDelta();
				}
			}

			public bool Contains(int x)
			{
				return x >= Bounds.X && x <= Bounds.X + Bounds.Width;
			}

			public override void GetProperties(ObjectPropertyList list)
			{
				if (_Book?.Deleted == false)
				{
					_Book.GetProperties(list);
				}
			}

			public override void AddNameProperty(ObjectPropertyList list)
			{
				if (_Book?.Deleted == false)
				{
					_Book.AddNameProperty(list);
				}
			}

			public override void AddNameProperties(ObjectPropertyList list)
			{
				if (_Book?.Deleted == false)
				{
					_Book.AddNameProperties(list);
				}
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.WriteEncodedInt(0);

				writer.WriteEncodedInt(Index);

				writer.Write(_Book);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadEncodedInt();

				Index = reader.ReadEncodedInt();

				_Book = reader.ReadItem<BaseBook>();
			}
		}

		private class ShelfUI : BaseGump
		{
			public DynamicBookshelf Shelf { get; }

			public BookDisplay Selected { get; private set; }

			public bool SuspendLayout { get; private set; }

			public ShelfUI(PlayerMobile user, DynamicBookshelf shelf)
				: base(user, 100, 100)
			{
				Shelf = shelf;

				TypeID = unchecked((Shelf.GetHashCode() * 397) ^ Shelf.Serial);

				if (Shelf._Gumps == null)
				{
					Shelf._Gumps = new Dictionary<Mobile, ShelfUI>();
				}
				else if (Shelf._Gumps.TryGetValue(User, out var oldUI))
				{
					oldUI.Close();
				}

				Shelf._Gumps[User] = this;
			}

			public override void OnDispose()
			{
				base.OnDispose();

				if (Shelf._Gumps.TryGetValue(User, out var ui) && ui == this)
				{
					Shelf._Gumps.Remove(User);

					if (Shelf._Gumps.Count == 0)
					{
						Shelf._Gumps = null;
					}
				}
			}

			public override void AddGumpLayout()
			{
				if (Selected.Book == null)
				{
					Selected = null;
				}

				AddPage(0);

				AddImage(0, 0, 495, Shelf.Hue);

				var bookX = 10;
				var bookY = 10;

				foreach (var c in Shelf.Components)
				{
					if (c is BookDisplay d)
					{
						var x = bookX + (d.Index * 19);
						var y = bookY;

						if (d.Book != null)
						{
							if (Selected == d)
							{
								AddImage(x, y, 22401, 0x33);
								AddButton(x, y, 22401, -1, () => BeginAddBook(d));

								AddTooltip("Take this book from the shelf");

								x -= 5;
								y += 5;
							}

							AddButton(x, y, d.GumpImage, d.GumpImage, () => SelectBook(d));
							AddImage(x, y, d.GumpImage, d.Hue);
							
							AddItemProperty(d.Book.Serial.Value);
						}
						else
						{
							if (Selected != null)
							{
								AddImage(x, y, 22400, 0x33);
								AddButton(x, y, 22400, -1, () => EndAddBook(d, Selected.Book));

								AddTooltip("Move the selected book to this slot");
							}
							else
							{
								AddImage(x, y, 22401, 0x33);
								AddButton(x, y, 22401, -1, () => BeginAddBook(d));

								AddTooltip("Add a book to this slot");
							}
						}
					}
				}
			}

			private void SelectBook(BookDisplay d)
			{
				if (Selected == d)
				{
					Selected = null;
				}
				else if (d.Book != null)
				{
					Selected = d;
				}

				Refresh();
			}

			private void BeginAddBook(BookDisplay d)
			{
				Refresh();

				if (d.Book == null)
				{
					User.SendMessage("Select a book to add to the shelf...");

					User.BeginTarget(-1, false, 0, EndAddBook, d);
				}
				else
				{
					User.SendMessage("A book is already occupying that slot.");
				}
			}

			private void EndAddBook(Mobile m, object target, BookDisplay d)
			{
				if (target is BaseBook book)
				{
					EndAddBook(d, book);
				}
				else
				{
					BeginAddBook(d);
				}
			}

			private void EndAddBook(BookDisplay d, BaseBook book)
			{
				if (d.Book == null)
				{
					SuspendLayout = true;

					book.Internalize();

					book.Location = new Point3D(d.Bounds.Start, 0);

					Shelf.AddItem(book);

					SuspendLayout = false;
				}
				else
				{
					User.SendMessage("A book is already occupying that slot.");
				}

				Refresh();
			}
		}
	}
}
