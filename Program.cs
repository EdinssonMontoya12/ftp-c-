using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace FTP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Ruta del servidor ftp
            string url = "ftp://192.168.100.23:9999/Download/R1CK__Y__M0RTY_01X01/";
            //Ruta de la carpeta donde se descargan los ficheros del s3
            string localDir = "U:\\Semestre 8\\Microservicios\\Previo 1 - 1151901";
            //Se valida que radicado sea diferente de vacio, aca no se aplica por fines de testing
            string radicado = "1151902";

            //Se obtinen la lista de archivos del servidor ftp
            string fileName = getListDirectoryFTP(url + radicado);
            //Si el retorno es igual al radicado la carpeta no existe
            if (!fileName.Equals(radicado + "\r\n"))
            {
                string[] names = fileName.Split('\n');

                for(int i = 0; i < names.Length-1; i++)
                    Console.WriteLine(url + names[i]);

                return;
            }

            //Se valida si en la base de datos existe en numero de radicado, sino retorna "El radicado no existe"
            //Caso contrario crea la carpeta en el servidor ftp y se trae los archivos del s3 y cargan al servidor ftp

            //Aca va la validacion a la base de datos *********************************************************************
            //Aca va la descargar y descompresion de archivos del s3 ******************************************************

            //Se crea la carptera en el servidor FTP
            if (!mkdFTP(url, radicado))
            {
                Console.WriteLine("Sorry, it is not possible to create a new folder.");
                return;
            }

            if (uploadFilesFTP(url + radicado, Directory.GetFiles(localDir)))
                return;
        }

        static bool uploadFilesFTP(string url, string[] filesNames)
        {
            try
            {
                WebClient client = new WebClient();
                string[] nameSplit = null;
                string name = "";
                string newUrl = "";

                foreach (string n in filesNames)
                {
                    nameSplit = n.Split('\\');
                    name = nameSplit[nameSplit.Length - 1];
                    newUrl = url + "/" + name;

                    client.UploadFile(newUrl, n);
                    client.DownloadFile(newUrl, $"C:/TEMPO/{name}");
                }

                return true;
            }catch(Exception e)
            {
                return false;
            }
        }

        static bool mkdFTP(string url, string directoryName)
        {
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(url + directoryName);
                req.KeepAlive = false;
                req.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse res = (FtpWebResponse)req.GetResponse();
                res.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        static string getListDirectoryFTP(string url)
        {
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(url);
                req.Method = WebRequestMethods.Ftp.ListDirectory;
                req.KeepAlive = false;

                FtpWebResponse res = (FtpWebResponse)req.GetResponse();

                Stream responseStream = res.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string response = reader.ReadToEnd();

                reader.Close();
                res.Close();
                return response;
            }
            catch (Exception ex)
            {
                return "Sorry, something as failed.";
            }
        }
    }
}
