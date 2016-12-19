using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PusherClient;
using System.Web;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private Image modelo_guia, modelo_factura, modelo_vale;

        //PUSHER

        static string _pusherKey = "36b16849a2a1aa3b5233";
        static Pusher _pusher = null;
        static Channel _myChannel = null;

        public Form1()
        {
            InitializeComponent();
            InitializePusher();

            modelo_guia = WindowsFormsApplication1.Properties.Resources.blanco_guia;
            modelo_factura = WindowsFormsApplication1.Properties.Resources.blanco_factura;
            modelo_vale = WindowsFormsApplication1.Properties.Resources.vale;
        }

        private void InitializePusher()
        {
            _pusher = new Pusher(_pusherKey, new PusherOptions() {
                Cluster = "eu",
                Authorizer = new HttpAuthorizer("http://combustible.ayp.com.pe/authorizer?channel_name=private-App.User.3&api_token=eA5zW0b5bd8x11hlYNBYepPv45ueNrq7rVszVmxC2eKzaEOY64QsykwR04hb")
            });

            _pusher.ConnectionStateChanged += _pusher_ConnectionStateChanged;
            _pusher.Error += _pusher_Error;
            _pusher.Connect();
            _myChannel = _pusher.Subscribe("private-App.User.3");
            _myChannel.Subscribed += _myChannel_suscribed;           
            _myChannel.Bind("Illuminate\\Notifications\\Events\\BroadcastNotificationCreated", (dynamic data) => {
               
                if (data.print_type == "invoice")
                {
                    string gallons = data.data.gallons;
                    string unit_price = data.data.unit_price;
                    string doc_number = data.data.doc_number;
                    string customer_name = data.data.customer_name;
                    string customer_address = data.data.customer_address;
                    string customer_phone = data.data.customer_phone;
                    string customer_ruc = data.data.customer_ruc;
                    string day = data.data.day;
                    string month = data.data.month;
                    string year = data.data.year;
                    string subtotal = data.data.subtotal;
                    string igv = data.data.igv;
                    string total = data.data.total;
                    string are = data.data.are;

                    if (MessageBox.Show("Deseas imprimir la factura?", "AYP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        imprimir(dibujarFactura(doc_number, customer_name, customer_address, customer_phone, customer_ruc, day, month, year, subtotal, igv, total, are, gallons, unit_price));
                    }

                }

                if (data.print_type == "shipping_document")
                {
                    string date = data.data.date;
                    string customer_name = data.data.customer_name;
                    string customer_ruc = data.data.customer_ruc;
                    string customer_address = data.data.customer_address;
                    string start_place = data.data.start_place;
                    string gallons = data.data.gallons;
                    string vehicle_plate = data.data.vehicle_plate;
                    string vehicle_brand = data.data.vehicle_brand;
                    string driver_licence = data.data.driver_licence;

                    if (MessageBox.Show("Deseas imprimir la guía?", "AYP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        imprimir(dibujarGuia(date, customer_name, customer_ruc, customer_address, start_place, gallons, driver_licence, vehicle_plate, vehicle_brand));
                    }
                }

                if (data.print_type == "sale")
                {
                    string customer_ruc = data.data.customer_ruc;
                    string customer_name = data.data.customer_name;
                    string day = data.data.day;
                    string month = data.data.month;
                    string year = data.data.year;
                    string gallons = data.data.gallons;
                    string vehicle_plate = data.data.vehicle_plate;
                    string driver_licence = data.data.driver_licence;
                    string gallon_counter_start = data.data.gallon_counter_start;
                    string gallon_counter_end = data.data.gallon_counter_end;

                    if (MessageBox.Show("Deseas imprimir el vale?", "AYP", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        imprimir(dibujarVale(gallon_counter_start, gallon_counter_end, day, month, year, vehicle_plate, driver_licence, customer_ruc, customer_name, gallons));
                    }
                }

            });
        }

        static void _pusher_ConnectionStateChanged(object sender, PusherClient.ConnectionState state) {

            Console.WriteLine("Estado de la conexión: " + state.ToString());
        }

        static void _pusher_Error(object sender, PusherException error)
        {
            Console.WriteLine("Error Pusher: " + error.ToString());
        }

        static void _myChannel_suscribed(object sender) {

            Console.WriteLine("Subscrito");
        }

        static void _presenceChannel_suscribed(object sender)
        {

            Console.WriteLine("Subscrito");
        }

        private Boolean imprimir(Image docActual)
        {

            PictureBox pbox = new PictureBox();
            pbox.Image = docActual;
            pbox.SizeMode = PictureBoxSizeMode.AutoSize;
            try
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += (sender, e) => e.Graphics.DrawImage(pbox.Image, 0, 0);
                pd.Print();
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Error al imprimir. Compruebe que la impresora esté conectada y encendida", "Advertencia");
                return false;
            }

        }

        private void imprimirEnPantalla(Image imagen)
        {
            Form f2 = new Form();
            f2.Show();
            f2.Validate();
            PictureBox pbox = new PictureBox();
            pbox.Image = imagen;
            pbox.SizeMode = PictureBoxSizeMode.AutoSize;
            f2.Width = pbox.Width + 17;
            f2.Height = pbox.Height + 37;
            f2.Controls.Add(pbox);
        }

        private Image dibujarVale(string gallon_counter_start, string gallon_counter_end, string day, string month, string year, string vehicle_plate, string driver_licence, string customer_ruc, string customer_name, string gallons)
        {
            Image valeActual = modelo_vale;
            Graphics g = Graphics.FromImage(valeActual);
            StringFormat formaler = new StringFormat();
            formaler.LineAlignment = StringAlignment.Near;
            formaler.Alignment = StringAlignment.Near;
            Font font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            Font font2 = new Font("Microsoft Sans Serif", 8, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            int datos_x = 47;
            int datos_y = 85;

            g.DrawString(gallon_counter_start, font, brush, new Point(datos_x + 95, datos_y), formaler);
            g.DrawString(gallon_counter_end, font, brush, new Point(datos_x + 98, datos_y + 23), formaler);

            g.DrawString(day, font, brush, new Point(datos_x + 350, datos_y + 3), formaler);
            g.DrawString(month, font, brush, new Point(datos_x + 385, datos_y + 3), formaler);
            g.DrawString(year, font, brush, new Point(datos_x + 419, datos_y + 3), formaler);

            g.DrawString(vehicle_plate, font2, brush, new Point(datos_x + 2, datos_y + 88), formaler);
            g.DrawString(driver_licence, font2, brush, new Point(datos_x + 58, datos_y + 88), formaler);
            g.DrawString(customer_ruc, font2, brush, new Point(datos_x + 235, datos_y + 88), formaler);
            g.DrawString(customer_name, font2, brush, new Point(datos_x + 58, datos_y + 108), formaler);
            g.DrawString(gallons, font2, brush, new Point(datos_x + 355, datos_y + 88), formaler);

            return valeActual;
        }
        

        private Image dibujarFactura(string doc_number, string customer_name, string customer_address, string customer_phone, string customer_ruc, string day, string month, string year, string subtotal, string igv, string total, string are, string gallons, string unit_price)
        {
            Image facturaActual = modelo_factura;
            Graphics g = Graphics.FromImage(facturaActual);
            StringFormat formaler = new StringFormat();
            formaler.LineAlignment = StringAlignment.Near;
            formaler.Alignment = StringAlignment.Near;
            Font font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            int datos_x = 112;
            int datos_y = 185;

            g.DrawString(customer_name, font, brush, new Point(datos_x, datos_y - 5), formaler);
            g.DrawString(customer_address, font, brush, new Point(datos_x, datos_y + 17), formaler);
            g.DrawString(customer_ruc, font, brush, new Point(datos_x, datos_y + 40), formaler);
            g.DrawString(customer_phone, font, brush, new Point(datos_x + 245, datos_y + 40), formaler);

            g.DrawString(day, font, brush, new Point(datos_x + 504, datos_y + 40), formaler);
            g.DrawString(month, font, brush, new Point(datos_x + 558, datos_y + 40), formaler);
            g.DrawString(year, font, brush, new Point(datos_x + 620, datos_y + 40), formaler);

            g.DrawString(gallons, font, brush, new Point(53, datos_y + 95), formaler);
            g.DrawString("DIESEL B5-S50", font, brush, new Point(datos_x, datos_y + 95), formaler);
            g.DrawString(unit_price, font, brush, new Point(datos_x + 485, datos_y + 95), formaler);
            g.DrawString(subtotal, font, brush, new Point(datos_x + 585, datos_y + 95), formaler);

            g.DrawString(are, font, brush, new Point(datos_x, datos_y + 282), formaler);


            g.DrawString(subtotal, font, brush, new Point(datos_x + 580, datos_y + 303), formaler);
            g.DrawString(igv, font, brush, new Point(datos_x + 580, datos_y + 324), formaler);
            g.DrawString(total, font, brush, new Point(datos_x + 580, datos_y + 348), formaler);

            return facturaActual;
        }

        private Image dibujarGuia(string date, string customer_name, string customer_ruc, string customer_address, string start_place, string gallons, string driver_licence, string vehicle_plate, string vehicle_brand)
        {
            Image GuiaActual = modelo_guia;
            Graphics g = Graphics.FromImage(GuiaActual);
            StringFormat formaler = new StringFormat();
            formaler.LineAlignment = StringAlignment.Near;
            formaler.Alignment = StringAlignment.Near;
            Font font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            int datos_x = 112;
            int datos_y = 186;

            g.DrawString(start_place, font, brush, new Point(50, datos_y + 3), formaler);
            g.DrawString(customer_address, font, brush, new Point(50 + 380, datos_y + 3), formaler);
            g.DrawString(date, font, brush, new Point(102, datos_y + 30), formaler);
            g.DrawString(date, font, brush, new Point(datos_x + 145, datos_y + 30), formaler);
            g.DrawString(customer_name, font, brush, new Point(datos_x + 5, datos_y + 50), formaler);
            g.DrawString(customer_ruc, font, brush, new Point(datos_x + 5, datos_y + 70), formaler);
            g.DrawString(vehicle_plate, font, brush, new Point(datos_x + 477, datos_y + 33), formaler);
            g.DrawString(driver_licence, font, brush, new Point(datos_x + 497, datos_y + 67), formaler);

            g.DrawString("DIESEL B5-S50", font, brush, new Point(115, datos_y + 196), formaler);
            g.DrawString(gallons, font, brush, new Point(540, datos_y + 196), formaler);
            g.DrawString("GL", font, brush, new Point(615, datos_y + 196), formaler);

            return GuiaActual;
         }
        
    }

}
