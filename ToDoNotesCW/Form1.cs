using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ToDoNotesCW
{
    /// <summary>
    /// Main form class that handles the To-Do List application functionality
    /// </summary>
    public partial class Form1 : Form
    {
        // Data structures to store task information
        private Dictionary<string, TaskItem> _tasks = new Dictionary<string, TaskItem>(); // Stores tasks with their details
        private List<string> _taskList = new List<string>(); // Maintains task order
        private HashSet<string> _markedTasks = new HashSet<string>(); // Tracks tasks marked for deletion
        private Timer timer; // Timer for updating datetime display
        private bool isEventHandlersInitialized = false; // Prevents duplicate event handler registration

        /// <summary>
        /// Constructor: Initializes the form and its components
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            InitializeEvents();
            InitializeTimer();
            UpdateDateTimeAndUser();
        }

        /// <summary>
        /// Initializes the timer for updating the datetime display
        /// </summary>
        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 1000; // Updates every second
            timer.Tick += (s, e) => UpdateDateTimeAndUser();
            timer.Start();
        }

        /// <summary>
        /// Updates the datetime and user information display
        /// </summary>
        private void UpdateDateTimeAndUser()
        {
            // Display current UTC time in specified format
            dateTimeLabel.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            userLabel.Text = "Current User: dimitar-panev";
        }

        /// <summary>
        /// Initializes all event handlers for the form controls
        /// </summary>
        private void InitializeEvents()
        {
            if (isEventHandlersInitialized) return;

            // Remove existing event handlers to prevent duplicates
            addButton.Click -= addButton_Click;
            toggleStatusButton.Click -= toggleStatusButton_Click;
            saveNotesButton.Click -= saveNotesButton_Click;
            deleteButton.Click -= deleteButton_Click;
            deleteMarkedButton.Click -= deleteMarkedButton_Click;
            markCheckBox.CheckedChanged -= markCheckBox_CheckedChanged;
            taskDisplay.SelectedIndexChanged -= taskDisplay_SelectedIndexChanged;

            // Add event handlers for all button clicks
            addButton.Click += addButton_Click;
            toggleStatusButton.Click += toggleStatusButton_Click;
            saveNotesButton.Click += saveNotesButton_Click;
            deleteButton.Click += deleteButton_Click;
            deleteMarkedButton.Click += deleteMarkedButton_Click;
            markCheckBox.CheckedChanged += markCheckBox_CheckedChanged;
            taskDisplay.SelectedIndexChanged += taskDisplay_SelectedIndexChanged;

            // Add hover effects for all buttons
            addButton.MouseEnter += Button_MouseEnter;
            addButton.MouseLeave += Button_MouseLeave;
            saveNotesButton.MouseEnter += Button_MouseEnter;
            saveNotesButton.MouseLeave += Button_MouseLeave;
            toggleStatusButton.MouseEnter += Button_MouseEnter;
            toggleStatusButton.MouseLeave += Button_MouseLeave;
            deleteButton.MouseEnter += Button_MouseEnter;
            deleteButton.MouseLeave += Button_MouseLeave;
            deleteMarkedButton.MouseEnter += Button_MouseEnter;
            deleteMarkedButton.MouseLeave += Button_MouseLeave;

            isEventHandlersInitialized = true;
        }

        /// <summary>
        /// Handles the addition of new tasks
        /// </summary>
        private void addButton_Click(object sender, EventArgs e)
        {
            // Validate task input
            if (string.IsNullOrWhiteSpace(taskInput.Text) || taskInput.Text == "Enter Task")
            {
                return;
            }

            string task = taskInput.Text.Trim();

            // Check for duplicate tasks
            if (_tasks.ContainsKey(task))
            {
                MessageBox.Show("Task name cannot be duplicate!");
                return;
            }

            // Add new task with initial pending status
            _tasks[task] = new TaskItem("Pending", "");
            _taskList.Add(task);
            UpdateTaskDisplay();
            UpdateAllNotesDisplay();
            taskInput.Clear();
        }

        /// <summary>
        /// Handles saving notes for the selected task
        /// </summary>
        private void saveNotesButton_Click(object sender, EventArgs e)
        {
            if (taskDisplay.SelectedItem == null) return;

            string selectedTask = GetSelectedTask();
            string notes = notesInput.Text;

            if (_tasks.ContainsKey(selectedTask))
            {
                _tasks[selectedTask].Notes = notes;
                UpdateAllNotesDisplay();
                MessageBox.Show($"Notes have been saved for {selectedTask}");
                notesInput.Clear();
            }
        }

        /// <summary>
        /// Toggles the status of the selected task between Pending and Completed
        /// </summary>
        private void toggleStatusButton_Click(object sender, EventArgs e)
        {
            if (taskDisplay.SelectedItem != null)
            {
                string selectedTask = GetSelectedTask();
                // Toggle task status
                if (_tasks[selectedTask].Status == "Pending")
                    _tasks[selectedTask].Status = "Completed";
                else
                    _tasks[selectedTask].Status = "Pending";
                UpdateTaskDisplay();
                UpdateSelectedTaskDetails();
            }
        }

        /// <summary>
        /// Updates UI when a different task is selected
        /// </summary>
        private void taskDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedTaskDetails();
            if (taskDisplay.SelectedItem != null)
            {
                string selectedTask = GetSelectedTask();
                markCheckBox.Checked = _markedTasks.Contains(selectedTask);
            }
        }

        /// <summary>
        /// Handles deletion of the selected task
        /// </summary>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (taskDisplay.SelectedItem != null)
            {
                string selectedTask = GetSelectedTask();
                // Remove task from all collections
                _tasks.Remove(selectedTask);
                _taskList.Remove(selectedTask);
                _markedTasks.Remove(selectedTask);
                UpdateTaskDisplay();
                UpdateAllNotesDisplay();
                UpdateSelectedTaskDetails();
            }
        }

        /// <summary>
        /// Handles marking/unmarking tasks for deletion
        /// </summary>
        private void markCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (taskDisplay.SelectedItem != null)
            {
                string selectedTask = GetSelectedTask();
                if (markCheckBox.Checked)
                    _markedTasks.Add(selectedTask);
                else
                    _markedTasks.Remove(selectedTask);
            }
        }

        /// <summary>
        /// Deletes all tasks that are marked for deletion
        /// </summary>
        private void deleteMarkedButton_Click(object sender, EventArgs e)
        {
            // Create a temporary list to avoid modification during enumeration
            var tasksToDelete = new List<string>(_markedTasks);
            foreach (string task in tasksToDelete)
            {
                _tasks.Remove(task);
                _taskList.Remove(task);
            }
            _markedTasks.Clear();
            UpdateTaskDisplay();
            UpdateAllNotesDisplay();
            UpdateSelectedTaskDetails();
        }

        /// <summary>
        /// Updates the task list display
        /// </summary>
        private void UpdateTaskDisplay()
        {
            taskDisplay.Items.Clear();
            foreach (string task in _taskList)
            {
                taskDisplay.Items.Add($"{task}: {_tasks[task].Status}");
            }
        }

        /// <summary>
        /// Updates the details display for the selected task
        /// </summary>
        private void UpdateSelectedTaskDetails()
        {
            if (taskDisplay.SelectedItem != null)
            {
                string selectedTask = GetSelectedTask();
                statusLabel.Text = $"Status: {_tasks[selectedTask].Status}";
                notesInput.Text = _tasks[selectedTask].Notes;
            }
            else
            {
                statusLabel.Text = "Status";
                notesInput.Text = "";
            }
        }

        /// <summary>
        /// Updates the display showing all task notes
        /// </summary>
        private void UpdateAllNotesDisplay()
        {
            allNotesDisplay.Clear();
            foreach (string task in _taskList)
            {
                if (_tasks.ContainsKey(task))
                {
                    string notes = _tasks[task].Notes?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(notes))
                    {
                        allNotesDisplay.AppendText($"Task: {task}\r\nNotes: {notes}\r\n\r\n");
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the task name from the selected item in the task display
        /// </summary>
        private string GetSelectedTask()
        {
            return taskDisplay.SelectedItem.ToString().Split(':')[0].Trim();
        }

        /// <summary>
        /// Handles the mouse enter event for buttons (hover effect)
        /// </summary>
        private void Button_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.BackColor = Color.FromArgb(53, 122, 189); // Darker blue on hover
            }
        }

        /// <summary>
        /// Handles the mouse leave event for buttons (hover effect)
        /// </summary>
        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.BackColor = Color.FromArgb(74, 144, 226); // Original blue color
            }
        }

        /// <summary>
        /// Cleanup when the form is closing
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }

    /// <summary>
    /// Class representing a task item with its status and associated notes
    /// </summary>
    public class TaskItem
    {
        public string Status { get; set; }
        public string Notes { get; set; }

        /// <summary>
        /// Constructor for creating a new task item
        /// </summary>
        /// <param name="status">Initial status of the task</param>
        /// <param name="notes">Initial notes for the task</param>
        public TaskItem(string status, string notes)
        {
            Status = status;
            Notes = notes ?? "";
        }
    }
}