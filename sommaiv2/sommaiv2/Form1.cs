using System; // ใช้งาน System namespace พื้นฐาน
using System.Collections.Generic; // ใช้งานการจัดการข้อมูลแบบ List/Collection
using System.Drawing; // ใช้งานระบบกราฟิกและการระบุพิกัด/สี
using System.Linq; // ใช้งานคำสั่งคัดกรองข้อมูล (LINQ) เช่น OfType, Where
using System.Windows.Forms; // ใช้งานส่วนประกอบหน้าต่างโปรแกรม (Windows Forms)

namespace sommaiv2 // ชื่อโปรเจกต์
{
    public partial class Form1 : Form // คลาสหลักของหน้าจอโปรแกรม
    {
        bool isPlayerTurn = true; // ตัวแปรเช็กว่าเป็นเทิร์นของผู้เล่นหรือไม่ (เริ่มต้นเป็น True)
        PictureBox? selectedPiece = null; // ตัวแปรเก็บค่าหมากที่กำลังถูกเลือก (ถ้าไม่มีจะเป็น null)
        int squareWidth, squareHeight; // ตัวแปรเก็บความกว้างและความสูงของแต่ละช่องบนกระดาน
        bool isMultiJumping = false; // ตัวแปรสถานะสำหรับการ "กินต่อเนื่อง" เพื่อล็อกไม่ให้เปลี่ยนตัวเล่น

        public Form1() // ฟังก์ชันเริ่มต้นเมื่อสร้างหน้าจอ
        {
            InitializeComponent(); // ตั้งค่าคอมโพเนนต์ต่างๆ ของหน้าจอ
            CalculateBoardSize(); // คำนวณขนาดช่องตารางให้เหมาะสมกับขนาดหน้าจอ
        }

        private void Form1_Load(object sender, EventArgs e) // เหตุการณ์เมื่อหน้าจอถูกโหลดขึ้นมา
        {
            CalculateBoardSize(); // คำนวณขนาดตารางอีกครั้งเพื่อป้องกันค่าเป็นศูนย์ (DivideByZero)
        }

        private void CalculateBoardSize() // ฟังก์ชันคำนวณขนาดช่องตาราง 8x8
        {
            squareWidth = Math.Max(this.ClientSize.Width / 8, 50); // หารความกว้างหน้าจอเป็น 8 ช่อง (ขั้นต่ำ 50 พิกเซล)
            squareHeight = Math.Max(this.ClientSize.Height / 8, 50); // หารความสูงหน้าจอเป็น 8 ช่อง (ขั้นต่ำ 50 พิกเซล)
        }

        // --- ระบบบังคับกิน & เดิน (สำหรับผู้เล่น) ---
        private void Form1_MouseClick(object sender, MouseEventArgs e) // เหตุการณ์เมื่อคลิกเมาส์บนหน้ากระดาน
        {
            if (!isPlayerTurn) return; // ถ้าไม่ใช่เทิร์นผู้เล่น (เทิร์น AI) ให้หยุดการทำงาน
            if (squareWidth <= 0 || squareHeight <= 0) CalculateBoardSize(); // ถ้าขนาดช่องยังไม่ถูกตั้งค่า ให้คำนวณใหม่

            int col = e.X / squareWidth; // คำนวณว่าคลิกที่คอลัมน์ไหน (แกน X)
            int row = e.Y / squareHeight; // คำนวณว่าคลิกที่แถวไหน (แกน Y)

            // 1. ตรรกะการเลือกหมาก (ผู้เล่นเล่นหมากดำ)
            var clickedPiece = GetPieceAt(row, col); // ตรวจสอบว่าในช่องที่คลิกมีหมากอยู่หรือไม่
            if (clickedPiece != null && clickedPiece.Tag?.ToString()?.Contains("Black") == true) // ถ้าเจอหมากและหมากนั้นเป็นสีดำ
            {
                if (isMultiJumping && clickedPiece != selectedPiece) return; // ถ้าอยู่ในช่วงกินต่อเนื่อง ต้องเลือกตัวเดิมเท่านั้น

                if (selectedPiece != null) selectedPiece.BackColor = Color.Transparent; // ล้างสีไฮไลต์ของหมากตัวเก่า (ถ้ามี)
                selectedPiece = clickedPiece; // ตั้งค่าหมากที่คลิกให้เป็นหมากที่ถูกเลือก
                selectedPiece.BackColor = Color.Cyan; // เปลี่ยนสีพื้นหลังเป็นสีฟ้าเพื่อให้รู้ว่าเลือกตัวนี้อยู่
                return; // จบการทำงานในขั้นตอนนี้
            }

            // 2. ตรรกะการสั่งเดินหมาก
            if (selectedPiece == null) return; // ถ้ายังไม่ได้เลือกหมากตัวไหนเลย ให้หยุดการทำงาน

            int startCol = (int)Math.Round((double)selectedPiece.Location.X / squareWidth); // หาคอลัมน์ปัจจุบันของหมากที่เลือก
            int startRow = (int)Math.Round((double)selectedPiece.Location.Y / squareHeight); // หาแถวปัจจุบันของหมากที่เลือก
            int rowDiff = row - startRow; // คำนวณระยะห่างแถวที่เดิน (ลบกัน)
            int colDiff = col - startCol; // คำนวณระยะห่างคอลัมน์ที่เดิน (ลบกัน)
            bool isKing = selectedPiece.Tag?.ToString()?.Contains("King") == true; // ตรวจสอบว่าหมากที่เลือกเป็น "ฮอส" หรือไม่

            // เช็กการคลิกนอกตาราง, คลิกทับตาขาว (หมากเดินได้เฉพาะตาดำ), หรือคลิกทับตาที่มีหมากอื่นอยู่แล้ว
            if (col < 0 || col >= 8 || row < 0 || row >= 8 || (row + col) % 2 == 0 || GetPieceAt(row, col) != null) return;

            bool mustCapture = CanPlayerCapture(); // เช็กว่าในเทิร์นนี้ผู้เล่นมีหมากที่ "ต้องบังคับกิน" หรือไม่

            if (!isKing) // --- กรณีเป็นหมากดำธรรมดา ---
            {
                if (rowDiff == -1 && Math.Abs(colDiff) == 1 && !isMultiJumping) // ถ้าเป็นการเดินปกติเฉียงๆ 1 ช่อง
                {
                    if (mustCapture) return; // ถ้ามีตาที่กินได้ แต่ไปเลือกเดินปกติ จะเดินไม่ได้ (กฎบังคับกิน)
                    MovePiece(selectedPiece, row, col); FinishTurn(); // ย้ายหมากและจบเทิร์น
                }
                else if (Math.Abs(rowDiff) == 2 && Math.Abs(colDiff) == 2) // ถ้าเป็นการเดินข้ามเพื่อกิน (ระยะ 2 ช่อง)
                {
                    if (rowDiff == -2) // เช็กว่าเป็นการเดินขึ้น (หมากดำเดินขึ้น)
                    {
                        var target = GetPieceAt(startRow + (rowDiff / 2), startCol + (colDiff / 2)); // หาหมากที่อยู่ตรงกลางระหว่างจุดเริ่มและจุดเป้าหมาย
                        if (target != null && target.Tag?.ToString()?.Contains("White") == true) // ถ้าหมากตรงกลางเป็นหมากขาว (ศัตรู)
                        {
                            ExecuteCapture(selectedPiece, target, row, col); // ทำการกินหมากตัวนั้น
                        }
                    }
                }
            }
            else // --- กรณีเป็นหมากดำที่เป็น "ฮอส" ---
            {
                if (Math.Abs(rowDiff) == Math.Abs(colDiff)) // ฮอสเดินเฉียงได้ทิศทางละกี่ช่องก็ได้ (แนวทแยง)
                {
                    var path = GetPathPieces(startRow, startCol, row, col); // หาหมากทั้งหมดที่ขวางอยู่ในเส้นทางเดิน
                    if (path.Count == 0 && !isMultiJumping) // ถ้าไม่มีหมากขวางเลย (ทางโล่ง) และไม่ได้อยู่ในช่วงกินต่อเนื่อง
                    {
                        if (mustCapture) return; // ถ้ามีตาฮอสที่กินได้ แต่เลือกเดินเฉยๆ จะเดินไม่ได้
                        MovePiece(selectedPiece, row, col); FinishTurn(); // ย้ายหมากฮอสและจบเทิร์น
                    }
                    else if (path.Count == 1 && path[0].Tag?.ToString()?.Contains("White") == true) // ถ้ามีหมากศัตรูขวางแค่ตัวเดียวในเส้นทาง
                    {
                        ExecuteCapture(selectedPiece, path[0], row, col); // ทำการกินหมากตัวที่ขวางอยู่นั้น
                    }
                }
            }
        }

        // ฟังก์ชันจัดการการกินหมาก (ใช้ร่วมกันทั้งหมากธรรมดาและฮอส)
        private void ExecuteCapture(PictureBox piece, PictureBox victim, int targetRow, int targetCol)
        {
            this.Controls.Remove(victim); // นำหมากที่ถูกกินออกจากหน้าจอ
            victim.Dispose(); // คืนหน่วยความจำของหมากที่ถูกลบ
            MovePiece(piece, targetRow, targetCol); // ย้ายหมากตัวกินไปยังตำแหน่งใหม่

            if (CanThisPieceCapture(piece)) // หลังจากกินแล้ว เช็กว่าตัวเดิมนี้ "กินตัวอื่นต่อได้อีกไหม"
            {
                isMultiJumping = true; // ตั้งค่าสถานะกินต่อเนื่อง
                piece.BackColor = Color.Red; // เปลี่ยนสีเป็นสีแดงเพื่อเตือนผู้เล่นว่าต้องกินต่อ
            }
            else // ถ้ากินต่อไม่ได้แล้ว
            {
                FinishTurn(); // จบเทิร์น
            }
        }

        // ฟังก์ชันจบเทิร์นผู้เล่น เพื่อส่งต่อให้ AI
        private void FinishTurn()
        {
            if (selectedPiece != null) selectedPiece.BackColor = Color.Transparent; // คืนสีพื้นหลังหมากให้เป็นปกติ
            selectedPiece = null; // ล้างค่าหมากที่เลือก
            isMultiJumping = false; // รีเซ็ตสถานะกินต่อเนื่อง
            isPlayerTurn = false; // เปลี่ยนสถานะเป็นเทิร์น AI

            System.Windows.Forms.Timer aiTimer = new System.Windows.Forms.Timer { Interval = 500 }; // สร้างตัวหน่วงเวลา 0.5 วินาทีเพื่อให้ดูสมจริง
            aiTimer.Tick += (s, e) => { aiTimer.Stop(); PerformAIMove(); }; // เมื่อครบเวลา ให้ AI เริ่มเดิน
            aiTimer.Start(); // เริ่มนับเวลา
        }

        // --- ระบบ AI (หมากขาว) ---
        private void PerformAIMove()
        {
            var whitePieces = this.Controls.OfType<PictureBox>() // ค้นหาหมากทั้งหมดในเครื่อง
                .Where(x => x.Tag?.ToString()?.Contains("White") == true && x.Visible).ToList(); // เลือกเฉพาะหมากขาวที่ยังไม่ถูกกิน
            if (whitePieces.Count == 0) return; // ถ้าหมากขาวหมดกระดาน ให้หยุดทำงาน

            Random rnd = new Random(); // สร้างตัวสุ่มสำหรับเลือกตาเดิน
            var possibleJumps = new List<(PictureBox piece, int tR, int tC, PictureBox victim)>(); // ลิสต์เก็บรายการที่กินได้
            var possibleMoves = new List<(PictureBox piece, int tR, int tC)>(); // ลิสต์เก็บรายการที่เดินได้ปกติ

            foreach (var p in whitePieces) // วนลูปเช็กหมากขาวทุกตัว
            {
                bool isKing = p.Tag?.ToString()?.Contains("King") == true; // เช็กว่าเป็นฮอสไหม
                int cCol = (int)Math.Round((double)p.Location.X / squareWidth); // ตำแหน่งคอลัมน์ปัจจุบัน
                int cRow = (int)Math.Round((double)p.Location.Y / squareHeight); // ตำแหน่งแถวปัจจุบัน
                int[] dirs = { -1, 1 }; // ทิศทางเฉียง (ซ้าย/ขวา)

                foreach (int rD in dirs) // ทิศทางแถว
                {
                    if (!isKing && rD == -1) continue; // หมากขาวธรรมดาห้ามเดินขึ้น (ถอยหลัง)
                    foreach (int cD in dirs) // ทิศทางคอลัมน์
                    {
                        bool foundOpponent = false; // ตัวแปรเช็กว่าเจอศัตรูขวางไหม
                        PictureBox? victim = null; // ตัวแปรเก็บหมากศัตรูที่เจอ
                        int maxDist = isKing ? 7 : 2; // ระยะการสแกน (ฮอสสแกนทั้งแถว, หมากธรรมดาสแกน 2 ช่อง)

                        for (int d = 1; d <= maxDist; d++) // เริ่มสแกนทีละช่อง
                        {
                            int tR = cRow + (rD * d), tC = cCol + (cD * d); // คำนวณตำแหน่งช่องเป้าหมาย
                            if (tR < 0 || tR >= 8 || tC < 0 || tC >= 8) break; // ถ้าออกนอกกระดานให้หยุดสแกนทิศนี้
                            var target = GetPieceAt(tR, tC); // ดูว่าช่องนั้นมีหมากไหม

                            if (!foundOpponent) // ถ้ายังไม่เจอศัตรูในทิศนี้
                            {
                                if (target == null) { if (d == 1 || isKing) possibleMoves.Add((p, tR, tC)); } // ถ้าช่องว่าง ให้เก็บเป็นตาเดินปกติ
                                else if (target.Tag?.ToString()?.Contains("Black") == true) { foundOpponent = true; victim = target; } // ถ้าเจอหมากดำ ให้จำไว้ว่าเป็นเหยื่อ
                                else break; // ถ้าเจอหมากพวกเดียวกันเอง ให้หยุดสแกนทิศนี้
                            }
                            else // ถ้าเจอศัตรูตัวก่อนหน้าแล้ว ช่องถัดไปต้องว่างถึงจะกินได้
                            {
                                if (target == null) { possibleJumps.Add((p, tR, tC, victim!)); if (!isKing) break; } // ถ้าว่าง เก็บเข้าลิสต์การกิน
                                else break; // ถ้าไม่ว่าง (มีหมากซ้อน) กินไม่ได้ ให้หยุด
                            }
                        }
                    }
                }
            }

            if (possibleJumps.Count > 0) // ถ้า AI มีตาที่กินได้ (บังคับกิน)
            {
                var choice = possibleJumps[rnd.Next(possibleJumps.Count)]; // สุ่มเลือกตากิน 1 อย่าง
                this.Controls.Remove(choice.victim); choice.victim.Dispose(); // ลบหมากดำที่โดนกิน
                MovePiece(choice.piece, choice.tR, choice.tC); // ย้ายหมากขาวไปที่ช่องใหม่

                if (CanThisPieceCapture(choice.piece)) // ถ้ากินต่อได้อีก
                {
                    System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 600 }; // หน่วงเวลาก่อนกินตัวถัดไป
                    t.Tick += (s, e) => { t.Stop(); PerformAIMove(); }; // เรียกฟังก์ชันเดิมเพื่อให้ AI ทำงานต่อ
                    t.Start(); // เริ่มตัวนับเวลา
                    return; // ยังไม่คืนเทิร์นให้ผู้เล่น
                }
            }
            else if (possibleMoves.Count > 0) // ถ้ากินไม่ได้แต่เดินได้ปกติ
            {
                var choice = possibleMoves[rnd.Next(possibleMoves.Count)]; // สุ่มเลือกตาเดิน
                MovePiece(choice.piece, choice.tR, choice.tC); // ย้ายหมาก
            }
            isMultiJumping = false; // รีเซ็ตสถานะกินต่อเนื่องของ AI
            isPlayerTurn = true; // คืนเทิร์นให้ผู้เล่น (หมากดำ)
        }

        // --- ฟังก์ชันช่วยเหลือ (Helper Functions) ---
        private bool CanPlayerCapture() // ตรวจสอบว่ามีหมากดำตัวไหนในกระดานกินได้บ้าง
        {
            return this.Controls.OfType<PictureBox>()
                .Where(x => x.Tag?.ToString()?.Contains("Black") == true && x.Visible)
                .Any(p => CanThisPieceCapture(p)); // ถ้ามีตัวไหนกินได้แม้แต่ตัวเดียวจะคืนค่า True
        }

        private bool CanThisPieceCapture(PictureBox p) // ตรวจสอบหมากรายตัวว่ากินศัตรูได้หรือไม่
        {
            bool isKing = p.Tag?.ToString()?.Contains("King") == true; // เช็กว่าเป็นฮอสไหม
            string opponent = p.Tag?.ToString()?.Contains("Black") == true ? "White" : "Black"; // กำหนดว่าศัตรูคือใคร
            int cCol = (int)Math.Round((double)p.Location.X / squareWidth); // คอลัมน์หมากตัวนี้
            int cRow = (int)Math.Round((double)p.Location.Y / squareHeight); // แถวหมากตัวนี้

            int[] dirs = { -1, 1 }; // ทิศทางเดิน
            foreach (int rD in dirs)
            {
                if (!isKing) // ถ้าไม่ใช่ฮอส
                {
                    if (p.Tag?.ToString()?.Contains("Black") == true && rD == 1) continue; // หมากดำธรรมดาห้ามกินลงล่าง
                    if (p.Tag?.ToString()?.Contains("White") == true && rD == -1) continue; // หมากขาวธรรมดาห้ามกินขึ้นบน
                }
                foreach (int cD in dirs)
                {
                    int maxDist = isKing ? 7 : 2; // ระยะเช็ก
                    for (int d = 2; d <= maxDist; d++) // เริ่มเช็กช่องที่ห่างออกไป 2 ช่อง
                    {
                        int tR = cRow + (rD * d), tC = cCol + (cD * d); // พิกัดช่องเป้าหมาย
                        if (tR < 0 || tR >= 8 || tC < 0 || tC >= 8) break; // ถ้านอกกระดานให้หยุด

                        if (GetPieceAt(tR, tC) == null) // ถ้าช่องเป้าหมายว่าง
                        {
                            var path = GetPathPieces(cRow, cCol, tR, tC); // ดูหมากระหว่างทาง
                            if (path.Count == 1 && path[0].Tag?.ToString()?.Contains(opponent) == true) return true; // ถ้าเจอศัตรู 1 ตัวพอดี แสดงว่ากินได้
                            if (path.Count > 1) break; // ถ้าเจอหมากขวางเกิน 1 ตัว กินไม่ได้
                        }
                        else break; // ถ้าช่องเป้าหมายไม่ว่าง กินไม่ได้
                    }
                }
            }
            return false; // สแกนทุกทิศแล้วกินไม่ได้เลย
        }

        private List<PictureBox> GetPathPieces(int sR, int sC, int tR, int tC) // ฟังก์ชันหาหมากที่อยู่ระหว่างทางเดินเฉียง
        {
            List<PictureBox> pieces = new List<PictureBox>(); // ลิสต์สำหรับเก็บหมากที่พบ
            int rDir = tR > sR ? 1 : -1, cDir = tC > sC ? 1 : -1; // กำหนดทิศทาง (+1 หรือ -1)
            int dist = Math.Abs(tR - sR); // ระยะห่างกี่ช่อง
            for (int i = 1; i < dist; i++) // วนลูปตามระยะห่าง (ไม่รวมจุดเริ่มและจุดจบ)
            {
                var p = GetPieceAt(sR + (i * rDir), sC + (i * cDir)); // ค้นหาหมากในพิกัดนั้นๆ
                if (p != null) pieces.Add(p); // ถ้าเจอหมากให้ใส่ในลิสต์
            }
            return pieces; // คืนค่าลิสต์หมากที่พบ
        }

        private PictureBox? GetPieceAt(int r, int c) // ฟังก์ชันหาหมากจากพิกัดแถวและคอลัมน์
        {
            return this.Controls.OfType<PictureBox>() // ค้นหา PictureBox ทั้งหมด
               .FirstOrDefault(p =>
            Math.Abs(p.Location.Y / (double)squareHeight - r) < 0.5 && // เช็กพิกัด Y ว่าอยู่แถวที่ระบุไหม (เผื่อค่าคลาดเคลื่อน 0.5)
            Math.Abs(p.Location.X / (double)squareWidth - c) < 0.5 && // เช็กพิกัด X ว่าอยู่คอลัมน์ที่ระบุไหม
            p.Visible); // ต้องเป็นหมากที่ยังมองเห็นได้ (ยังไม่โดนกิน)
        }

        private void MovePiece(PictureBox p, int r, int c) // ฟังก์ชันย้ายหมากไปยังตำแหน่งใหม่
        {
            p.Location = new Point(c * squareWidth, r * squareHeight); // ตั้งค่าพิกัด Location ใหม่ตามช่องที่คำนวณได้

            // เช็กการเดินสุดกระดานเพื่อเปลี่ยนเป็น "ฮอส"
            if (p.Tag != null && ((p.Tag.ToString() == "Black" && r == 0) || (p.Tag.ToString() == "White" && r == 7)))
            {
                p.Tag = p.Tag.ToString() + "King"; // เปลี่ยนค่า Tag ให้มีคำว่า King
                // เปลี่ยนรูปภาพหมากให้เป็นรูปฮอส (ดึงจาก Resources ที่คุณอัปโหลดไว้)
                p.Image = (p.Tag.ToString()!.Contains("Black")) ? Properties.Resources.หมากดำhos : Properties.Resources.หมากขาวhos;
            }
        }
    }
}