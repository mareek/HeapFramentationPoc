using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeapFramentationPoc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowInfos();
        }

        private void AllocateButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                AllocateOnLargeObjectHeap();
            }
            ShowInfos();
        }

        private void FreeSimpleButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                TryFreeMemorySimple();
            }
            ShowInfos();
        }

        private void FreeEnhancedButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                TryFreeMemoryEnhanced();
            }
            ShowInfos();
        }

        private void ShowInfos()
        {
            OutputTextBox.Text =
$@"Arrays allocated : {_hoarder.Count}
Total size allocated : {_hoarder.Sum(a => a.Length * SizeOfStruct) / 1024:#,###} KB
GC total memory : {GC.GetTotalMemory(false) / 1024:#,###} KB
Working set : {Process.GetCurrentProcess().WorkingSet64 / 1024:#,###} KB";
        }

        private static readonly int SizeOfStruct = GetSizeOfStruct();
        private static int GetSizeOfStruct()
        {
            unsafe
            {
                return sizeof(MySomewhatLargeStruct);
            }
        }

        private readonly List<MySomewhatLargeStruct[]> _hoarder = new List<MySomewhatLargeStruct[]>();

        private void AllocateOnLargeObjectHeap()
        {
            const int minSize = 2_000;
            const int maxSize = 5_000;
            var rng = new Random();
            int allocateSize = rng.Next(minSize, maxSize);
            _hoarder.Insert(rng.Next(_hoarder.Count + 1), MySomewhatLargeStruct.AllocateStructArray(allocateSize));
        }

        private void TryFreeMemoryEnhanced()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            TryFreeMemorySimple();
        }

        private void TryFreeMemorySimple()
        {
            if (_hoarder.Any())
            {
                _hoarder.RemoveAt(0);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

            }
        }

        private struct MySomewhatLargeStruct
        {
            public Guid Id { get; }
            public DateTime Date { get; }
            public long Ticks { get; }
            public long SequenceNumber { get; }
            public double RandomNumber { get; }

            public MySomewhatLargeStruct(long sequenceNumber, double randomNumber)
            {
                Id = Guid.NewGuid();
                Date = DateTime.Now;
                Ticks = Date.Ticks;
                SequenceNumber = sequenceNumber;
                RandomNumber = randomNumber;
            }

            public static MySomewhatLargeStruct[] AllocateStructArray(int size)
            {
                var rng = new Random();
                var result = new MySomewhatLargeStruct[size];
                for (int i = 0; i < size; i++)
                {
                    result[i] = new MySomewhatLargeStruct(i, rng.NextDouble());
                }

                return result;
            }
        }
    }
}
