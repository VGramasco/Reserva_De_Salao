using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace Reserva_Salao
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LimparBusca();
            CarregarDadosMoradores();
            CarregarDadosReservas(); 
        }

        private void CarregarDadosMoradores()
        {
            string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();

            using (MySqlConnection conexao = new MySqlConnection(connString))
            {
                try
                {
                    conexao.Open();
                    string query = "SELECT * FROM morador";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexao);
                    DataTable tabelaMoradores = new DataTable();
                    adapter.Fill(tabelaMoradores);

                    dataGridView1.DataSource = tabelaMoradores;
                    dataGridView2.DataSource = tabelaMoradores;

                    if (dataGridView1.Rows.Count > 0)
                    {
                        dataGridView1.ClearSelection();
                    }

                    if (dataGridView2.Rows.Count > 0)
                    {
                        dataGridView2.ClearSelection();
                    }
                }
                catch (MySqlException msqle)
                {
                    MessageBox.Show("Erro de Acesso ao banco MySQL: " + msqle.Message, "Erro");
                }
            }
        }

        private void CarregarDadosReservas()
        {
            string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();

            using (MySqlConnection conexao = new MySqlConnection(connString))
            {
                try
                {
                    conexao.Open();
                    string query = "SELECT * FROM reservas_salao";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexao);
                    DataTable tabelaReservas = new DataTable();
                    adapter.Fill(tabelaReservas);

                    dataGridView3.DataSource = tabelaReservas;

                    if (dataGridView3.Rows.Count > 0)
                    {
                        dataGridView3.ClearSelection();
                    }
                }
                catch (MySqlException msqle)
                {
                    MessageBox.Show("Erro de Acesso ao banco MySQL: " + msqle.Message, "Erro");
                }
            }
        }

        private void dataGridView1_SelectionChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                NomeMorador.Text = row.Cells["Nome"].Value.ToString();
                Apartamento.Text = row.Cells["Apartamento"].Value.ToString();
            }
        }

        private void dataGridView3_SelectionChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView3.SelectedRows[0];
                NomeEvento.Text = row.Cells["Nome_Evento"].Value.ToString();
                DataReserva.Value = Convert.ToDateTime(row.Cells["Data_Agendamento"].Value);
                QntPessoas.Value = Convert.ToInt32(row.Cells["Quantidade_Pessoas"].Value);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomeMorador.Text))
            {
                MessageBox.Show("Nome do morador não informado!");
                return;
            }
            if (string.IsNullOrWhiteSpace(Apartamento.Text))
            {
                MessageBox.Show("Apartamento não informado!");
                return;
            }

            string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();

            using (MySqlConnection conexao = new MySqlConnection(connString))
            {
                try
                {
                    conexao.Open();

                    string query = "INSERT INTO morador (Nome, Apartamento) VALUES (@vNome, @vApartamento)";
                    using (MySqlCommand comando = new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("@vNome", NomeMorador.Text.Trim());
                        comando.Parameters.AddWithValue("@vApartamento", Apartamento.Text.Trim());

                        int valorRetorno = comando.ExecuteNonQuery();

                        if (valorRetorno < 1)
                        {
                            MessageBox.Show("Erro ao cadastrar. Tente novamente.");
                        }
                        else
                        {
                            MessageBox.Show("Cadastro realizado com sucesso!");
                            CarregarDadosMoradores();
                        }
                    }

                    MySqlTransaction transaction = conexao.BeginTransaction();
                    transaction.Commit();
                }
                catch (MySqlException msqle)
                {
                    MessageBox.Show("Erro de Acesso ao banco MySQL: " + msqle.Message, "Erro");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dataReserva = DataReserva.Value.Date;
            string nomeEvento = NomeEvento.Text.Trim();
            int qntPessoas = Convert.ToInt32(QntPessoas.Value);

            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView2.SelectedRows[0];
                int idMorador = Convert.ToInt32(row.Cells["Id"].Value);

                if (string.IsNullOrWhiteSpace(nomeEvento))
                {
                    MessageBox.Show("Nome do evento não informado!");
                    return;
                }
                if (qntPessoas <= 0)
                {
                    MessageBox.Show("Quantidade de pessoas inválida!");
                    return;
                }

                string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();
                using (MySqlConnection conexao = new MySqlConnection(connString))
                {
                    try
                    {
                        conexao.Open();

                        string verificaQuery = "SELECT COUNT(*) FROM reservas_salao WHERE DATE(Data_Agendamento) = @dataReserva";
                        using (MySqlCommand verificaComando = new MySqlCommand(verificaQuery, conexao))
                        {
                            verificaComando.Parameters.AddWithValue("@dataReserva", dataReserva);

                            int count = Convert.ToInt32(verificaComando.ExecuteScalar());
                            if (count > 0)
                            {
                                MessageBox.Show("Já existe uma reserva para essa data. Escolha outra data.");
                                return;
                            }
                        }

                        string query = "INSERT INTO reservas_salao (Nome_Evento, Data_Agendamento, Quantidade_Pessoas, idMorador) " +
                                       "VALUES (@nomeEvento, @dataReserva, @qntPessoas, @idMorador)";
                        using (MySqlCommand comando = new MySqlCommand(query, conexao))
                        {
                            comando.Parameters.AddWithValue("@nomeEvento", nomeEvento);
                            comando.Parameters.AddWithValue("@dataReserva", dataReserva);
                            comando.Parameters.AddWithValue("@qntPessoas", qntPessoas);
                            comando.Parameters.AddWithValue("@idMorador", idMorador);

                            int rowsAffected = comando.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reserva realizada com sucesso!");
                                LimparBusca();
                                CarregarDadosReservas();
                            }
                            else
                            {
                                MessageBox.Show("Erro ao realizar a reserva. Tente novamente.");
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhum morador selecionado para fazer a reserva.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();
                using (MySqlConnection conexao = new MySqlConnection(connString))
                {
                    try
                    {
                        conexao.Open();
                        DataGridViewRow row = dataGridView1.SelectedRows[0];
                        string query = "UPDATE morador SET Nome = @vNome, Apartamento = @vApartamento WHERE Id = @vId";
                        using (MySqlCommand comando = new MySqlCommand(query, conexao))
                        {
                            comando.Parameters.AddWithValue("@vNome", NomeMorador.Text.Trim());
                            comando.Parameters.AddWithValue("@vApartamento", Apartamento.Text.Trim());
                            comando.Parameters.AddWithValue("@vId", row.Cells["Id"].Value);

                            int valorRetorno = comando.ExecuteNonQuery();

                            if (valorRetorno < 1)
                            {
                                MessageBox.Show("Erro ao atualizar. Tente novamente.");
                            }
                            else
                            {
                                MessageBox.Show("Atualização realizada com sucesso!");
                                CarregarDadosMoradores();
                            }
                        }
                    }
                    catch (MySqlException msqle)
                    {
                        MessageBox.Show("Erro de Acesso ao banco MySQL: " + msqle.Message, "Erro");
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhuma linha selecionada para atualizar.");
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                int moradorId = Convert.ToInt32(row.Cells["Id"].Value);

                string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();
                using (MySqlConnection conexao = new MySqlConnection(connString))
                {
                    try
                    {
                        conexao.Open();

                        string verificaReservasQuery = "SELECT COUNT(*) FROM reservas_salao WHERE idMorador = @moradorId";
                        using (MySqlCommand verificaReservasComando = new MySqlCommand(verificaReservasQuery, conexao))
                        {
                            verificaReservasComando.Parameters.AddWithValue("@moradorId", moradorId);

                            int reservasCount = Convert.ToInt32(verificaReservasComando.ExecuteScalar());

                            if (reservasCount > 0)
                            {
                                DialogResult result = MessageBox.Show("Este morador possui reservas associadas. Deseja excluir o morador juntamente com suas reservas?", "Reservas Encontradas", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                                if (result == DialogResult.No)
                                {
                                    return;
                                }
                            }
                        }

                        string deleteReservasQuery = "DELETE FROM reservas_salao WHERE idMorador = @moradorId";
                        using (MySqlCommand deleteReservasComando = new MySqlCommand(deleteReservasQuery, conexao))
                        {
                            deleteReservasComando.Parameters.AddWithValue("@moradorId", moradorId);
                            deleteReservasComando.ExecuteNonQuery();
                        }

                        string deleteMoradorQuery = "DELETE FROM morador WHERE Id = @moradorId";
                        using (MySqlCommand deleteMoradorComando = new MySqlCommand(deleteMoradorQuery, conexao))
                        {
                            deleteMoradorComando.Parameters.AddWithValue("@moradorId", moradorId);
                            int valorRetorno = deleteMoradorComando.ExecuteNonQuery();

                            if (valorRetorno < 1)
                            {
                                MessageBox.Show("Erro ao excluir o morador. Tente novamente.");
                            }
                            else
                            {
                                MessageBox.Show("Morador excluído com sucesso!");
                                CarregarDadosMoradores();
                                CarregarDadosReservas();
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhum morador selecionado para excluir.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView3.SelectedRows[0];
                int idReserva = Convert.ToInt32(row.Cells["Id"].Value);

                string nomeEvento = NomeEvento.Text.Trim();
                DateTime dataReserva = DataReserva.Value.Date;
                int qntPessoas = Convert.ToInt32(QntPessoas.Value);

                if (string.IsNullOrWhiteSpace(nomeEvento))
                {
                    MessageBox.Show("Nome do evento não informado!");
                    return;
                }
                if (qntPessoas <= 0)
                {
                    MessageBox.Show("Quantidade de pessoas inválida!");
                    return;
                }

                string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();
                using (MySqlConnection conexao = new MySqlConnection(connString))
                {
                    try
                    {
                        conexao.Open();
                        string query = "UPDATE reservas_salao SET Nome_Evento = @nomeEvento, Data_Agendamento = @dataReserva, Quantidade_Pessoas = @qntPessoas WHERE Id = @idReserva";
                        using (MySqlCommand comando = new MySqlCommand(query, conexao))
                        {
                            comando.Parameters.AddWithValue("@nomeEvento", nomeEvento);
                            comando.Parameters.AddWithValue("@dataReserva", dataReserva);
                            comando.Parameters.AddWithValue("@qntPessoas", qntPessoas);
                            comando.Parameters.AddWithValue("@idReserva", idReserva);

                            int rowsAffected = comando.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reserva atualizada com sucesso!");
                                CarregarDadosReservas();
                            }
                            else
                            {
                                MessageBox.Show("Erro ao atualizar a reserva. Tente novamente.");
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhuma reserva selecionada para atualizar.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView3.SelectedRows[0];
                int idReserva = Convert.ToInt32(row.Cells["Id"].Value);

                string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();
                using (MySqlConnection conexao = new MySqlConnection(connString))
                {
                    try
                    {
                        conexao.Open();
                        string query = "DELETE FROM reservas_salao WHERE Id = @idReserva";
                        using (MySqlCommand comando = new MySqlCommand(query, conexao))
                        {
                            comando.Parameters.AddWithValue("@idReserva", idReserva);

                            int rowsAffected = comando.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reserva excluída com sucesso!");
                                CarregarDadosReservas();
                            }
                            else
                            {
                                MessageBox.Show("Erro ao excluir a reserva. Tente novamente.");
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhuma reserva selecionada para excluir.");
            }
        }

        private void BuscarPorData(DateTime data)
        {
            string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();

            using (MySqlConnection conexao = new MySqlConnection(connString))
            {
                try
                {
                    conexao.Open();
                    string query = "SELECT * FROM reservas_salao WHERE Data_Agendamento = @data";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexao);
                    adapter.SelectCommand.Parameters.AddWithValue("@data", data.Date);
                    DataTable tabelaReservas = new DataTable();
                    adapter.Fill(tabelaReservas);

                    dataGridView3.DataSource = tabelaReservas;

                    if (dataGridView3.Rows.Count > 0)
                    {
                        dataGridView3.ClearSelection();
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message);
                }
            }
        }

        private void LimparBusca()
        {
            string connString = ConfigurationManager.ConnectionStrings["MySQLConnectionString"].ToString();

            using (MySqlConnection conexao = new MySqlConnection(connString))
            {
                try
                {
                    conexao.Open();
                    string query = "SELECT * FROM reservas_salao";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexao);
                    DataTable tabelaReservas = new DataTable();
                    adapter.Fill(tabelaReservas);

                    dataGridView3.DataSource = tabelaReservas;

                    if (dataGridView3.Rows.Count > 0)
                    {
                        dataGridView3.ClearSelection();
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DateTime data = dateTimePicker2.Value.Date;
            BuscarPorData(data);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            LimparBusca();
        }
    }
}