using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Lab_5;

public class MainForm : Form
{
    private readonly TextBox txtId = new();
    private readonly TextBox txtTitle = new();
    private readonly RichTextBox txtText = new();
    private readonly PictureBox pictureBox = new();
    private readonly DataGridView grid = new();
    private readonly Label lblStatus = new();

    private byte[]? imageData;

    public MainForm()
    {
        Text = "Lab_5 — Збережені процедури";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1100;
        Height = 720;
        MinimumSize = new Size(950, 600);

        BuildUi();
        LoadBooks();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        var leftPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(5)
        };

        leftPanel.Controls.Add(new Label
        {
            Text = "ID книги для перевірки або видалення:",
            AutoSize = true
        });

        txtId.Width = 380;
        leftPanel.Controls.Add(txtId);

        leftPanel.Controls.Add(new Label
        {
            Text = "Назва книги:",
            AutoSize = true
        });

        txtTitle.Width = 380;
        leftPanel.Controls.Add(txtTitle);

        leftPanel.Controls.Add(new Label
        {
            Text = "Текстова примітка:",
            AutoSize = true
        });

        txtText.Width = 380;
        txtText.Height = 110;
        txtText.ScrollBars = RichTextBoxScrollBars.Vertical;
        leftPanel.Controls.Add(txtText);

        leftPanel.Controls.Add(new Label
        {
            Text = "Зображення:",
            AutoSize = true
        });

        pictureBox.Width = 380;
        pictureBox.Height = 190;
        pictureBox.BorderStyle = BorderStyle.FixedSingle;
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        leftPanel.Controls.Add(pictureBox);

        var imageButtons = new FlowLayoutPanel
        {
            AutoSize = true,
            Width = 380,
            WrapContents = true
        };

        var btnLoadImage = new Button
        {
            Text = "Завантажити зображення",
            AutoSize = true
        };
        btnLoadImage.Click += (_, _) => LoadImage();

        var btnAdd = new Button
        {
            Text = "Додати книгу",
            AutoSize = true
        };
        btnAdd.Click += (_, _) => AddBook();

        imageButtons.Controls.Add(btnLoadImage);
        imageButtons.Controls.Add(btnAdd);
        leftPanel.Controls.Add(imageButtons);

        var actionButtons = new FlowLayoutPanel
        {
            AutoSize = true,
            Width = 380,
            WrapContents = true
        };

        var btnDelete = new Button
        {
            Text = "Видалити за ID",
            AutoSize = true
        };
        btnDelete.Click += (_, _) => DeleteBook();

        var btnCheck = new Button
        {
            Text = "Перевірити за ID",
            AutoSize = true
        };
        btnCheck.Click += (_, _) => CheckBook();

        var btnRefresh = new Button
        {
            Text = "Оновити таблицю",
            AutoSize = true
        };
        btnRefresh.Click += (_, _) => LoadBooks();

        var btnClear = new Button
        {
            Text = "Очистити",
            AutoSize = true
        };
        btnClear.Click += (_, _) => ClearInputs();

        actionButtons.Controls.Add(btnDelete);
        actionButtons.Controls.Add(btnCheck);
        actionButtons.Controls.Add(btnRefresh);
        actionButtons.Controls.Add(btnClear);
        leftPanel.Controls.Add(actionButtons);

        lblStatus.AutoSize = true;
        lblStatus.MaximumSize = new Size(380, 0);
        leftPanel.Controls.Add(lblStatus);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoGenerateColumns = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Id",
            HeaderText = "ID",
            DataPropertyName = "Id",
            FillWeight = 12
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Title",
            HeaderText = "Назва",
            DataPropertyName = "Title",
            FillWeight = 28
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Text",
            HeaderText = "Примітка",
            DataPropertyName = "Text",
            FillWeight = 60
        });

        grid.CellClick += Grid_CellClick;

        root.Controls.Add(leftPanel, 0, 0);
        root.Controls.Add(grid, 1, 0);

        Controls.Add(root);
    }

    private void LoadBooks()
    {
        const string sql = "SELECT Id, Title, [Text], ImageData FROM dbo.Books ORDER BY Id;";

        try
        {
            using var connection = new SqlConnection(DbConfig.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            using var adapter = new SqlDataAdapter(command);

            var table = new DataTable();
            adapter.Fill(table);
            grid.DataSource = table;
            lblStatus.Text = $"Записів у таблиці: {table.Rows.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка завантаження даних:\n{ex.Message}",
                "Помилка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void LoadImage()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Зображення|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            DisplayImage(File.ReadAllBytes(dialog.FileName));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не вдалося відкрити зображення:\n{ex.Message}",
                "Помилка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void DisplayImage(byte[]? bytes)
    {
        if (pictureBox.Image != null)
        {
            Image oldImage = pictureBox.Image;
            pictureBox.Image = null;
            oldImage.Dispose();
        }

        imageData = bytes;

        if (bytes == null || bytes.Length == 0)
            return;

        using var stream = new MemoryStream(bytes);
        using var source = Image.FromStream(stream);
        pictureBox.Image = new Bitmap(source);
    }

    private void AddBook()
    {
        string title = txtTitle.Text.Trim();
        string text = txtText.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Введіть назву книги.");
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            MessageBox.Show("Введіть текстову примітку.");
            return;
        }

        if (imageData == null || imageData.Length == 0)
        {
            MessageBox.Show("Завантажте зображення.");
            return;
        }

        try
        {
            using var connection = new SqlConnection(DbConfig.ConnectionString);
            using var command = new SqlCommand("dbo.AddBook", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Title", SqlDbType.NVarChar, 100).Value = title;
            command.Parameters.Add("@Text", SqlDbType.NVarChar, -1).Value = text;
            command.Parameters.Add("@ImageData", SqlDbType.VarBinary, -1).Value = imageData;

            connection.Open();
            command.ExecuteNonQuery();

            LoadBooks();
            ClearInputs();
            lblStatus.Text = "Книгу успішно додано.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка додавання:\n{ex.Message}",
                "Помилка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void DeleteBook()
    {
        if (!int.TryParse(txtId.Text.Trim(), out int id))
        {
            MessageBox.Show("Введіть ID книги або виберіть запис у таблиці.");
            return;
        }

        if (MessageBox.Show(
                $"Видалити книгу з ID {id}?",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using var connection = new SqlConnection(DbConfig.ConnectionString);
            using var command = new SqlCommand("dbo.DeleteBookById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            var deletedRows = command.Parameters.Add("@DeletedRows", SqlDbType.Int);
            deletedRows.Direction = ParameterDirection.Output;

            connection.Open();
            command.ExecuteNonQuery();

            int count = Convert.ToInt32(deletedRows.Value);
            LoadBooks();
            ClearInputs();

            lblStatus.Text = count == 1
                ? $"Книгу з ID {id} видалено."
                : $"Книгу з ID {id} не знайдено.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка видалення:\n{ex.Message}",
                "Помилка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void CheckBook()
    {
        if (!int.TryParse(txtId.Text.Trim(), out int id))
        {
            MessageBox.Show("Введіть коректний ID книги.");
            return;
        }

        try
        {
            using var connection = new SqlConnection(DbConfig.ConnectionString);
            using var command = new SqlCommand("dbo.CheckBookById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            var existsParameter = command.Parameters.Add("@Exists", SqlDbType.Bit);
            existsParameter.Direction = ParameterDirection.Output;

            var titleParameter = command.Parameters.Add("@Title", SqlDbType.NVarChar, 100);
            titleParameter.Direction = ParameterDirection.Output;

            connection.Open();
            command.ExecuteNonQuery();

            bool exists = Convert.ToBoolean(existsParameter.Value);

            if (exists)
            {
                string title = titleParameter.Value?.ToString() ?? "";
                lblStatus.Text = $"Знайдено: {title}";

                MessageBox.Show(
                    $"Книгу знайдено.\nID: {id}\nНазва: {title}",
                    "Результат перевірки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                lblStatus.Text = $"Книгу з ID {id} не знайдено.";

                MessageBox.Show(
                    $"Книги з ID {id} не існує.",
                    "Результат перевірки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка перевірки:\n{ex.Message}",
                "Помилка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        if (grid.Rows[e.RowIndex].DataBoundItem is not DataRowView row)
            return;

        txtId.Text = row["Id"].ToString() ?? "";
        txtTitle.Text = row["Title"].ToString() ?? "";
        txtText.Text = row["Text"].ToString() ?? "";

        if (row["ImageData"] is byte[] bytes)
            DisplayImage(bytes);
        else
            DisplayImage(null);
    }

    private void ClearInputs()
    {
        txtId.Clear();
        txtTitle.Clear();
        txtText.Clear();
        DisplayImage(null);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (pictureBox.Image != null)
        {
            pictureBox.Image.Dispose();
            pictureBox.Image = null;
        }

        base.OnFormClosed(e);
    }
}
