using System;
using System.Data.SQLite;
using System.Windows.Forms;
using AdminSERMAC.Services;

namespace AdminSERMAC.Forms
{
    public class CrearProductoForm : Form
    {
        private TextBox codigoTextBox;
        private TextBox nombreTextBox;
        private TextBox marcaTextBox;
        private ComboBox unidadMedidaComboBox;
        private ComboBox categoriaComboBox;
        private ComboBox subcategoriaComboBox;
        private Button guardarButton;
        private SQLiteService sqliteService;
        private ILogger<CrearProductoForm> logger;

        public CrearProductoForm(ILogger<CrearProductoForm> logger)
        {
            this.logger = logger;
            this.Text = "Crear Producto";
            this.Width = 400;
            this.Height = 450;

            sqliteService = new SQLiteService(logger as ILogger<SQLiteService>);

            InitializeComponents();
            CargarCategorias();
        }


        private void InitializeComponents()
        {
            Label codigoLabel = new Label { Text = "Código:", Top = 20, Left = 20, Width = 100 };
            codigoTextBox = new TextBox { Top = 20, Left = 130, Width = 200 };

            Label nombreLabel = new Label { Text = "Nombre:", Top = 60, Left = 20, Width = 100 };
            nombreTextBox = new TextBox { Top = 60, Left = 130, Width = 200 };

            Label marcaLabel = new Label { Text = "Marca:", Top = 100, Left = 20, Width = 100 };
            marcaTextBox = new TextBox { Top = 100, Left = 130, Width = 200 };

            Label unidadMedidaLabel = new Label { Text = "Unidad de Medida:", Top = 140, Left = 20, Width = 120 };
            unidadMedidaComboBox = new ComboBox
            {
                Top = 140,
                Left = 150,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            unidadMedidaComboBox.Items.Add("Kg");
            unidadMedidaComboBox.Items.Add("Unidades");
            unidadMedidaComboBox.SelectedIndex = 0; // Selecciona "Kg" por defecto

            Label categoriaLabel = new Label { Text = "Categoría:", Top = 180, Left = 20, Width = 100 };
            categoriaComboBox = new ComboBox { Top = 180, Left = 130, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label subcategoriaLabel = new Label { Text = "Subcategoría:", Top = 220, Left = 20, Width = 100 };
            subcategoriaComboBox = new ComboBox { Top = 220, Left = 130, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            guardarButton = new Button
            {
                Text = "Guardar",
                Top = 280,
                Left = 130,
                Width = 100,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            guardarButton.Click += GuardarButton_Click;

            this.Controls.AddRange(new Control[]
            {
                codigoLabel, codigoTextBox,
                nombreLabel, nombreTextBox,
                marcaLabel, marcaTextBox,
                unidadMedidaLabel, unidadMedidaComboBox,
                categoriaLabel, categoriaComboBox,
                subcategoriaLabel, subcategoriaComboBox,
                guardarButton
            });
        }

        private void CargarCategorias()
        {
            categoriaComboBox.Items.Clear();
            categoriaComboBox.Items.Add("Seleccionar");

            var categorias = sqliteService.GetCategorias();
            if (categorias != null)
            {
                categoriaComboBox.Items.AddRange(categorias.ToArray());
            }

            categoriaComboBox.SelectedIndex = 0;
            categoriaComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (categoriaComboBox.SelectedIndex > 0)
                {
                    var subcategorias = sqliteService.GetSubCategorias(categoriaComboBox.SelectedItem.ToString());
                    subcategoriaComboBox.Items.Clear();
                    subcategoriaComboBox.Items.Add("Seleccionar");
                    subcategoriaComboBox.Items.AddRange(subcategorias.ToArray());
                    subcategoriaComboBox.SelectedIndex = 0;
                }
            };
        }

        private void GuardarButton_Click(object sender, EventArgs e)
        {
            string codigo = codigoTextBox.Text.Trim();
            string nombre = nombreTextBox.Text.Trim();
            string marca = marcaTextBox.Text.Trim();
            string unidadMedida = unidadMedidaComboBox.SelectedItem?.ToString();
            string categoria = categoriaComboBox.SelectedItem?.ToString();
            string subcategoria = subcategoriaComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(marca) ||
                unidadMedida == "Seleccionar" || categoria == "Seleccionar" || subcategoria == "Seleccionar")
            {
                MessageBox.Show("Por favor, completa todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(sqliteService.connectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(
                        "INSERT INTO Productos (Codigo, Nombre, Marca, UnidadMedida, Categoria, SubCategoria) VALUES (@codigo, @nombre, @marca, @unidadMedida, @categoria, @subcategoria)",
                        connection);
                    command.Parameters.AddWithValue("@codigo", codigo);
                    command.Parameters.AddWithValue("@nombre", nombre);
                    command.Parameters.AddWithValue("@marca", marca);
                    command.Parameters.AddWithValue("@unidadMedida", unidadMedida);
                    command.Parameters.AddWithValue("@categoria", categoria);
                    command.Parameters.AddWithValue("@subcategoria", subcategoria);

                    command.ExecuteNonQuery();
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
