using System.Windows;
using System.Windows.Controls;

namespace Timetable
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContentHost.Content = new HomePage();
            SetNavigationButtonActive(HomeNavButton);
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            Button? clickedButton = sender as Button;
            if (clickedButton == null) return;

            SetNavigationButtonActive(clickedButton);
            LoadContentForNavigation(clickedButton.Content.ToString());
        }

        private void SetNavigationButtonActive(Button activeButton)
        {
            HomeNavButton.Style = (Style)FindResource("NavButton");
            InstructorNavButton.Style = (Style)FindResource("NavButton");
            RoomNavButton.Style = (Style)FindResource("NavButton");
            MeetingTimeNavButton.Style = (Style)FindResource("NavButton");
            CourseNavButton.Style = (Style)FindResource("NavButton");
            GroupNavButton.Style = (Style)FindResource("NavButton");
            TimeTableNavButton.Style = (Style)FindResource("NavButton");

            activeButton.Style = (Style)FindResource("NavButtonActive");
        }

        private void LoadContentForNavigation(string? buttonContent)
        {
            UserControl? contentToLoad = null;

            switch (buttonContent)
            {
                case "Home":
                    contentToLoad = new HomePage();
                    break;
                case "Instructors":
                    contentToLoad = new InstructorPage();
                    break;
                case "Rooms":
                    contentToLoad = new RoomPage();
                    break;
                case "Courses":
                    contentToLoad = new CoursePage();
                    break;
                case "Meetings":
                    contentToLoad = new MeetingPage();
                    break;
                case "Groups":
                    contentToLoad = new GroupPage();
                    break;
                case "TimeTable":
                    contentToLoad = new TimetablePage();
                    break;
                default:
                    MessageBox.Show($"Navigation to '{buttonContent}' not implemented.", "Error");
                    break;
            }

            if (contentToLoad != null)
            {
                MainContentHost.Content = contentToLoad;
            }
        }

        private void InstructorView(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new InstructorPage();
        }
    }
}