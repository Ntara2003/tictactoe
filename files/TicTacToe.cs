using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TicTacToe
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm());
        }
    }

    // ─── Game Logic ────────────────────────────────────────────────────────────
    class GameEngine
    {
        public char[] Board { get; private set; } = new char[9];
        public char CurrentPlayer { get; private set; } = 'X';
        public bool GameOver { get; private set; } = false;
        public int[] WinLine { get; private set; } = null;
        public int ScoreX { get; private set; } = 0;
        public int ScoreO { get; private set; } = 0;
        public int Draws { get; private set; } = 0;
        public string Mode { get; set; } = "pvp"; // "pvp" or "cpu"

        private static readonly int[][] Lines =
        {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
            new[]{0,4,8}, new[]{2,4,6}
        };

        public void Reset()
        {
            Board = new char[9];
            CurrentPlayer = 'X';
            GameOver = false;
            WinLine = null;
        }

        public void ResetAll()
        {
            ScoreX = ScoreO = Draws = 0;
            Reset();
        }

        /// <summary>Returns result: "win", "draw", "continue"</summary>
        public string Play(int index)
        {
            if (GameOver || Board[index] != '\0') return "invalid";

            Board[index] = CurrentPlayer;

            var win = CheckWin(Board);
            if (win != null)
            {
                WinLine = win;
                GameOver = true;
                if (CurrentPlayer == 'X') ScoreX++;
                else ScoreO++;
                return "win";
            }

            if (IsFull(Board))
            {
                GameOver = true;
                Draws++;
                return "draw";
            }

            CurrentPlayer = CurrentPlayer == 'X' ? 'O' : 'X';
            return "continue";
        }

        public int GetCpuMove()
        {
            return Minimax(Board, 'O').Index;
        }

        private (int Score, int Index) Minimax(char[] b, char player)
        {
            var win = CheckWin(b);
            if (win != null) return (b[win[0]] == 'O' ? 10 : -10, -1);
            if (IsFull(b)) return (0, -1);

            int bestScore = player == 'O' ? int.MinValue : int.MaxValue;
            int bestIdx = -1;

            for (int i = 0; i < 9; i++)
            {
                if (b[i] != '\0') continue;
                var nb = (char[])b.Clone();
                nb[i] = player;
                int score = Minimax(nb, player == 'O' ? 'X' : 'O').Score;
                if (player == 'O' ? score > bestScore : score < bestScore)
                {
                    bestScore = score;
                    bestIdx = i;
                }
            }
            return (bestScore, bestIdx);
        }

        private int[] CheckWin(char[] b)
        {
            foreach (var line in Lines)
                if (b[line[0]] != '\0' && b[line[0]] == b[line[1]] && b[line[0]] == b[line[2]])
                    return line;
            return null;
        }

        private bool IsFull(char[] b)
        {
            foreach (var c in b) if (c == '\0') return false;
            return true;
        }
    }

    // ─── Main Form ─────────────────────────────────────────────────────────────
    class GameForm : Form
    {
        private GameEngine engine = new GameEngine();
        private Button[] cells = new Button[9];
        private Label lblStatus, lblScoreX, lblScoreO, lblDraws;
        private Button btnPvP, btnCPU, btnNew, btnReset;
        private System.Windows.Forms.Timer cpuTimer;

        // Palette
        private Color bgColor     = Color.FromArgb(15, 12, 30);
        private Color surfaceColor = Color.FromArgb(28, 22, 55);
        private Color accentX     = Color.FromArgb(99, 179, 237);   // sky blue
        private Color accentO     = Color.FromArgb(252, 129, 74);   // coral
        private Color winColor    = Color.FromArgb(72, 199, 142);   // mint
        private Color textPrimary = Color.FromArgb(240, 235, 255);
        private Color textMuted   = Color.FromArgb(140, 130, 175);
        private Color borderColor = Color.FromArgb(60, 50, 100);

        public GameForm()
        {
            Text = "Tic Tac Toe";
            Size = new Size(480, 620);
            MinimumSize = new Size(480, 620);
            MaximumSize = new Size(480, 620);
            BackColor = bgColor;
            ForeColor = textPrimary;
            Font = new Font("Segoe UI", 10f);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUI();

            cpuTimer = new System.Windows.Forms.Timer { Interval = 400 };
            cpuTimer.Tick += (s, e) => { cpuTimer.Stop(); DoCpuMove(); };
        }

        private void BuildUI()
        {
            int pad = 24;
            int w = ClientSize.Width;

            // ── Title ──
            var title = new Label
            {
                Text = "TIC TAC TOE",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = textPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, 20, w, 40)
            };
            Controls.Add(title);

            // ── Scores ──
            int sy = 72;
            lblScoreX = MakeScoreLabel("0", accentX,  pad + 30, sy, 80);
            var lx    = MakeScoreCaption("X",      pad + 50, sy + 30);
            lblDraws  = MakeScoreLabel("0", textMuted, w/2 - 30, sy, 60);
            var ld    = MakeScoreCaption("DRAW",   w/2 - 22, sy + 30);
            lblScoreO = MakeScoreLabel("0", accentO,  w - pad - 110, sy, 80);
            var lo    = MakeScoreCaption("O",      w - pad - 90, sy + 30);

            Controls.AddRange(new Control[]{ lblScoreX, lx, lblDraws, ld, lblScoreO, lo });

            // ── Mode buttons ──
            int my = 130;
            btnPvP = MakeModeButton("2 Players", pad, my, 190);
            btnCPU = MakeModeButton("vs Computer", w/2 + 4, my, 192);
            btnPvP.Click += (s,e) => SetMode("pvp");
            btnCPU.Click += (s,e) => SetMode("cpu");
            btnPvP.BackColor = Color.FromArgb(50, 40, 90);
            Controls.AddRange(new Control[]{ btnPvP, btnCPU });

            // ── Status label ──
            lblStatus = new Label
            {
                Text = "X's turn",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = accentX,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds = new Rectangle(0, 168, w, 28)
            };
            Controls.Add(lblStatus);

            // ── Board ──
            int cellSize = 120;
            int boardW   = cellSize * 3;
            int bx       = (w - boardW) / 2;
            int by       = 204;

            for (int i = 0; i < 9; i++)
            {
                int row = i / 3, col = i % 3;
                var btn = new Button
                {
                    Bounds    = new Rectangle(bx + col*cellSize, by + row*cellSize, cellSize, cellSize),
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 36f, FontStyle.Bold),
                    BackColor = surfaceColor,
                    ForeColor = textPrimary,
                    Cursor    = Cursors.Hand,
                    Tag       = i
                };
                btn.FlatAppearance.BorderColor = borderColor;
                btn.FlatAppearance.BorderSize  = 1;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 30, 70);
                btn.Click += CellClick;
                cells[i] = btn;
                Controls.Add(btn);
            }

            // ── Action buttons ──
            int ay = by + cellSize * 3 + 20;
            btnNew   = MakeActionButton("New Game",     bx,          ay, 174);
            btnReset = MakeActionButton("Reset Scores", bx + 186,    ay, 174);
            btnNew.Click   += (s,e) => { engine.Reset(); RefreshBoard(); };
            btnReset.Click += (s,e) => { engine.ResetAll(); RefreshBoard(); };
            Controls.AddRange(new Control[]{ btnNew, btnReset });
        }

        // ── Cell Click ──────────────────────────────────────────────────────────
        private void CellClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int idx = (int)btn.Tag;
            if (engine.GameOver || engine.Board[idx] != '\0') return;
            if (engine.Mode == "cpu" && engine.CurrentPlayer == 'O') return;

            string result = engine.Play(idx);
            RefreshBoard();

            if (result == "continue" && engine.Mode == "cpu")
                cpuTimer.Start();
        }

        private void DoCpuMove()
        {
            if (engine.GameOver) return;
            engine.Play(engine.GetCpuMove());
            RefreshBoard();
        }

        // ── Refresh UI ──────────────────────────────────────────────────────────
        private void RefreshBoard()
        {
            for (int i = 0; i < 9; i++)
            {
                char c = engine.Board[i];
                cells[i].Text      = c == '\0' ? "" : c.ToString();
                cells[i].ForeColor = c == 'X' ? accentX : accentO;
                cells[i].BackColor = surfaceColor;
                cells[i].Enabled   = !engine.GameOver && c == '\0';
            }

            if (engine.WinLine != null)
                foreach (int wi in engine.WinLine)
                    cells[wi].BackColor = Color.FromArgb(30, 199, 142, 100);

            // Status
            if (engine.GameOver)
            {
                if (engine.WinLine != null)
                {
                    string winner = engine.Board[engine.WinLine[0]].ToString();
                    lblStatus.Text = $"{winner} wins! 🎉";
                    lblStatus.ForeColor = winner == "X" ? accentX : accentO;
                }
                else
                {
                    lblStatus.Text = "It's a draw!";
                    lblStatus.ForeColor = textMuted;
                }
            }
            else
            {
                string cpu = (engine.Mode == "cpu" && engine.CurrentPlayer == 'O') ? " (computer)" : "";
                lblStatus.Text      = $"{engine.CurrentPlayer}'s turn{cpu}";
                lblStatus.ForeColor = engine.CurrentPlayer == 'X' ? accentX : accentO;
            }

            // Scores
            lblScoreX.Text = engine.ScoreX.ToString();
            lblScoreO.Text = engine.ScoreO.ToString();
            lblDraws.Text  = engine.Draws.ToString();
        }

        private void SetMode(string mode)
        {
            engine.Mode = mode;
            btnPvP.BackColor = mode == "pvp" ? Color.FromArgb(50, 40, 90) : surfaceColor;
            btnCPU.BackColor = mode == "cpu" ? Color.FromArgb(50, 40, 90) : surfaceColor;
            engine.Reset();
            RefreshBoard();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private Label MakeScoreLabel(string text, Color color, int x, int y, int w)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = color,
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds    = new Rectangle(x, y, w, 34)
            };
        }

        private Label MakeScoreCaption(string text, int x, int y)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = textMuted,
                AutoSize  = true,
                BackColor = Color.Transparent,
                Location  = new Point(x, y)
            };
        }

        private Button MakeModeButton(string text, int x, int y, int width)
        {
            var b = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(x, y, width, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = surfaceColor,
                ForeColor = textMuted,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = borderColor;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 30, 70);
            return b;
        }

        private Button MakeActionButton(string text, int x, int y, int width)
        {
            var b = new Button
            {
                Text      = text,
                Bounds    = new Rectangle(x, y, width, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 32, 80),
                ForeColor = textPrimary,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = borderColor;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 50, 110);
            return b;
        }
    }
}
