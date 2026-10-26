using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AES_Crypto_App
{
    public partial class Form1 : Form
    {
        private byte[] currentKey;
        private byte[] currentIV;
        private string decryptedFilePath = "";

        // Khai báo các thành phần Giao diện
        private Button btnGenerateKey, btnSaveKey, btnLoadKey;
        private Label lblKeyStatus, lblStatus;
        private TextBox txtInputText, txtOutputBase64, txtFilePath;
        private Button btnEncryptText, btnBrowse, btnEncryptFile, btnDecryptFile, btnCompareBytes;
        private ProgressBar progressBar1;

        public Form1()
        {
            InitializeComponent();
            InitCustomGUI(); // Tự động tạo giao diện
        }

        // HÀM SỰ KIỆN NÀY ĐÃ ĐƯỢC CHÈN BỔ SUNG ĐỂ SỬA LỖI CS0103
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // TỰ ĐỘNG DỰNG GIAO DIỆN NGUYÊN BẢN KHÔNG CẦN KÉO THẢ
        private void InitCustomGUI()
        {
            this.Text = "Ứng dụng Mã hóa AES-256 (An toàn & Bảo mật thông tin)";
            this.Size = new Size(640, 530);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. Nhóm Quản lý Khóa
            GroupBox gbKey = new GroupBox { Text = " 1 - 3. Quản lý Khóa AES ", Location = new Point(12, 10), Size = new Size(600, 95) };
            btnGenerateKey = new Button { Text = "1. Sinh khóa AES", Location = new Point(15, 25), Size = new Size(175, 30) };
            btnSaveKey = new Button { Text = "2. Lưu khóa ra File", Location = new Point(205, 25), Size = new Size(175, 30) };
            btnLoadKey = new Button { Text = "3. Import khóa", Location = new Point(395, 25), Size = new Size(175, 30) };
            lblKeyStatus = new Label { Text = "Trạng thái: Chưa có khóa", Location = new Point(15, 65), AutoSize = true, ForeColor = Color.Red };
            gbKey.Controls.AddRange(new Control[] { btnGenerateKey, btnSaveKey, btnLoadKey, lblKeyStatus });

            // 2. Nhóm Mã hóa Văn bản
            GroupBox gbText = new GroupBox { Text = " 4. Mã hóa Chuỗi văn bản ", Location = new Point(12, 115), Size = new Size(600, 135) };
            Label lbl1 = new Label { Text = "Văn bản gốc:", Location = new Point(15, 22), AutoSize = true };
            txtInputText = new TextBox { Text = "Hệ thống An toàn thông tin - Bài tập Mã hóa AES 256-bit", Location = new Point(15, 42), Size = new Size(560, 23) };
            btnEncryptText = new Button { Text = "4. Mã hóa chuỗi", Location = new Point(15, 72), Size = new Size(140, 30) };
            Label lbl2 = new Label { Text = "Kết quả Base64:", Location = new Point(165, 78), AutoSize = true };
            txtOutputBase64 = new TextBox { Location = new Point(265, 75), Size = new Size(310, 23), ReadOnly = true };
            gbText.Controls.AddRange(new Control[] { lbl1, txtInputText, btnEncryptText, lbl2, txtOutputBase64 });

            // 3. Nhóm Mã hóa File
            GroupBox gbFile = new GroupBox { Text = " 5. Mã hóa & Giải mã File dung lượng lớn (GB) ", Location = new Point(12, 260), Size = new Size(600, 195) };
            Label lbl3 = new Label { Text = "Đường dẫn File:", Location = new Point(15, 22), AutoSize = true };
            txtFilePath = new TextBox { Location = new Point(15, 42), Size = new Size(455, 23), ReadOnly = true };
            btnBrowse = new Button { Text = "Chọn File GB...", Location = new Point(480, 40), Size = new Size(95, 26) };

            btnEncryptFile = new Button { Text = "5. Mã hóa File", Location = new Point(15, 75), Size = new Size(175, 35) };
            btnDecryptFile = new Button { Text = "Giải mã File", Location = new Point(205, 75), Size = new Size(175, 35) };
            btnCompareBytes = new Button { Text = "So sánh Bytes", Location = new Point(395, 75), Size = new Size(180, 35) };

            progressBar1 = new ProgressBar { Location = new Point(15, 120), Size = new Size(560, 20) };
            lblStatus = new Label { Text = "Sẵn sàng", Location = new Point(15, 150), AutoSize = true, Font = new Font(this.Font, FontStyle.Italic) };
            gbFile.Controls.AddRange(new Control[] { lbl3, txtFilePath, btnBrowse, btnEncryptFile, btnDecryptFile, btnCompareBytes, progressBar1, lblStatus });

            this.Controls.AddRange(new Control[] { gbKey, gbText, gbFile });

            // Gắn sự kiện nút bấm
            btnGenerateKey.Click += btnGenerateKey_Click;
            btnSaveKey.Click += btnSaveKey_Click;
            btnLoadKey.Click += btnLoadKey_Click;
            btnEncryptText.Click += btnEncryptText_Click;
            btnBrowse.Click += btnBrowse_Click;
            btnEncryptFile.Click += btnEncryptFile_Click;
            btnDecryptFile.Click += btnDecryptFile_Click;
            btnCompareBytes.Click += btnCompareBytes_Click;
        }

        // --- LOGIC THỰC THI THUẬT TOÁN ---

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                currentKey = aes.Key;
                currentIV = aes.IV;
            }
            lblKeyStatus.Text = "Trạng thái: Đã sinh khóa 256-bit mới!";
            lblKeyStatus.ForeColor = Color.Green;
        }

        private void btnSaveKey_Click(object sender, EventArgs e)
        {
            if (currentKey == null) { MessageBox.Show("Chưa có khóa để lưu!"); return; }

            SaveFileDialog sfd = new SaveFileDialog { Filter = "DAT File|*.dat|All Files|*.*" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(sfd.FileName, FileMode.Create)))
                {
                    bw.Write(currentKey.Length);
                    bw.Write(currentKey);
                    bw.Write(currentIV.Length);
                    bw.Write(currentIV);
                }
                MessageBox.Show("Đã lưu khóa ra file thành công!");
            }
        }

        private void btnLoadKey_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "DAT File|*.dat|All Files|*.*" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (BinaryReader br = new BinaryReader(File.Open(ofd.FileName, FileMode.Open)))
                {
                    int keyLen = br.ReadInt32();
                    currentKey = br.ReadBytes(keyLen);
                    int ivLen = br.ReadInt32();
                    currentIV = br.ReadBytes(ivLen);
                }
                lblKeyStatus.Text = "Trạng thái: Đã nạp khóa từ file thành công!";
                lblKeyStatus.ForeColor = Color.Blue;
            }
        }

        private void btnEncryptText_Click(object sender, EventArgs e)
        {
            if (currentKey == null) { MessageBox.Show("Vui lòng sinh hoặc nạp khóa trước!"); return; }

            using (Aes aes = Aes.Create())
            {
                aes.Key = currentKey;
                aes.IV = currentIV;
                ICryptoTransform encryptor = aes.CreateEncryptor();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(txtInputText.Text);
                    }
                    txtOutputBase64.Text = Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "All Files|*.*" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = ofd.FileName;
            }
        }

        private async void btnEncryptFile_Click(object sender, EventArgs e)
        {
            if (currentKey == null || string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Vui lòng chọn file và nạp khóa trước!");
                return;
            }

            string inputFile = txtFilePath.Text;
            string outputFile = inputFile + ".enc";
            long totalBytes = new FileInfo(inputFile).Length;

            progressBar1.Value = 0;
            lblStatus.Text = "Đang mã hóa file...";

            Stopwatch sw = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = currentKey;
                    aes.IV = currentIV;
                    using (FileStream fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] buffer = new byte[1024 * 1024]; // 1MB
                        int bytesRead;
                        long readSoFar = 0;

                        while ((bytesRead = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, bytesRead);
                            readSoFar += bytesRead;

                            int progress = (int)((double)readSoFar / totalBytes * 100);
                            Invoke(new Action(() => progressBar1.Value = Math.Min(100, progress)));
                        }
                    }
                }
            });

            sw.Stop();
            lblStatus.Text = $"Mã hóa xong! Thời gian: {sw.ElapsedMilliseconds / 1000.0:F2} giây.";
        }

        private async void btnDecryptFile_Click(object sender, EventArgs e)
        {
            if (currentKey == null || string.IsNullOrEmpty(txtFilePath.Text)) return;

            string encryptedFile = txtFilePath.Text + ".enc";
            decryptedFilePath = txtFilePath.Text + ".dec";

            if (!File.Exists(encryptedFile))
            {
                MessageBox.Show("Không tìm thấy file mã hóa (.enc)!");
                return;
            }

            long totalBytes = new FileInfo(encryptedFile).Length;
            progressBar1.Value = 0;
            lblStatus.Text = "Đang giải mã file...";

            Stopwatch sw = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = currentKey;
                    aes.IV = currentIV;
                    using (FileStream fsIn = new FileStream(encryptedFile, FileMode.Open, FileAccess.Read))
                    using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream fsOut = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        int bytesRead;
                        long readSoFar = 0;

                        while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsOut.Write(buffer, 0, bytesRead);
                            readSoFar += bytesRead;

                            int progress = (int)((double)readSoFar / totalBytes * 100);
                            Invoke(new Action(() => progressBar1.Value = Math.Min(100, progress)));
                        }
                    }
                }
            });

            sw.Stop();
            lblStatus.Text = $"Giải mã xong! Thời gian: {sw.ElapsedMilliseconds / 1000.0:F2} giây.";
        }

        private async void btnCompareBytes_Click(object sender, EventArgs e)
        {
            string originalFile = txtFilePath.Text;
            if (string.IsNullOrEmpty(originalFile) || string.IsNullOrEmpty(decryptedFilePath) || !File.Exists(decryptedFilePath))
            {
                MessageBox.Show("Hãy thực hiện mã hóa và giải mã file trước!");
                return;
            }

            lblStatus.Text = "Đang kiểm tra tính toàn vẹn bytes...";
            bool isMatch = false;

            await Task.Run(() =>
            {
                FileInfo info1 = new FileInfo(originalFile);
                FileInfo info2 = new FileInfo(decryptedFilePath);

                if (info1.Length != info2.Length)
                {
                    isMatch = false;
                    return;
                }

                using (FileStream fs1 = new FileStream(originalFile, FileMode.Open, FileAccess.Read))
                using (FileStream fs2 = new FileStream(decryptedFilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buf1 = new byte[1024 * 1024];
                    byte[] buf2 = new byte[1024 * 1024];
                    int count1;

                    while ((count1 = fs1.Read(buf1, 0, buf1.Length)) > 0)
                    {
                        fs2.Read(buf2, 0, buf2.Length);
                        for (int i = 0; i < count1; i++)
                        {
                            if (buf1[i] != buf2[i])
                            {
                                isMatch = false;
                                return;
                            }
                        }
                    }
                }
                isMatch = true;
            });

            if (isMatch)
            {
                lblStatus.Text = "So sánh xong: TRÙNG KHỚP 100%";
                MessageBox.Show("KẾT QUẢ: TRÙNG KHỚP 100% (Dữ liệu bảo toàn tuyệt đối)", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "So sánh xong: KHÔNG TRÙNG KHỚP";
                MessageBox.Show("KẾT QUẢ: KHÔNG TRÙNG KHỚP", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}