using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace StriveHitboxes
{
    class OverlaidImage
    {
        int MARGIN = 0;
        const string JONBIN_HEADER = "JONB";
        bool is_gg = false;
        bool is_uni = false;
        uint height;
        uint width;
        uint pxoffsetx;
        uint pxoffsety;
        uint texwidth;
        uint texheight;
        uint hitboxcount;
        uint hurtboxcount;
        public List<JonbinBox> hurtboxes;
        public List<JonbinBox> hitboxes;
        public List<JonbinChunk> chunks;
        Bitmap hipsprite;
        Bitmap marginbitmap;
        public Bitmap boxbitmap;
        Color HURT_COLOR = Color.FromArgb(100, 0, 0, 255);
        Color HIT_COLOR = Color.FromArgb(100, 255, 0, 0);
        public OverlaidImage(string colpacpath, int coloffset)
        {
            is_gg = true;
            parseJONB(colpacpath, coloffset);
            renderBoxes();
        }
        private void parseJONB(string colpacpath, int coloffset)
        {
            BinaryReader jonbr = new BinaryReader(new FileStream(colpacpath, FileMode.Open));
            jonbr.BaseStream.Seek((long)coloffset, SeekOrigin.Begin);
            if (!jonbr.ReadChars(4).SequenceEqual(JONBIN_HEADER.ToCharArray()))
            {
                Console.WriteLine("NOT A JONBIN FILE");
                return;
            }
            Console.WriteLine("Image Names: ");
            short imcount = jonbr.ReadInt16();
            for (var i = 0; i < imcount; i++)
            {
                var strbytes = jonbr.ReadBytes(0x20).TakeWhile(b => (b != 0)).ToArray();
                Console.WriteLine(Encoding.UTF8.GetString(strbytes, 0, strbytes.Length));
            }
            jonbr.BaseStream.Seek(3, SeekOrigin.Current);
            var chunkcount = jonbr.ReadUInt32();
            hurtboxcount = (uint)jonbr.ReadInt16();
            hitboxcount = (uint)jonbr.ReadInt16();
            chunks = new List<JonbinChunk>();
            hurtboxes = new List<JonbinBox>();
            hitboxes = new List<JonbinBox>();
            jonbr.BaseStream.Seek(41 * 2 + 2, SeekOrigin.Current);
            if (imcount > 0)
            {
                for (var c = 0; c < chunkcount; c++)
                {
                    var chunk = new JonbinChunk();
                    chunk.SrcX = jonbr.ReadSingle();
                    chunk.SrcY = jonbr.ReadSingle();
                    chunk.SrcWidth = jonbr.ReadSingle();
                    chunk.SrcHeight = jonbr.ReadSingle();
                    chunk.DestX = jonbr.ReadSingle();
                    chunk.DestY = jonbr.ReadSingle();
                    chunk.DestWidth = jonbr.ReadSingle();
                    chunk.DestHeight = jonbr.ReadSingle();
                    jonbr.BaseStream.Seek(0x20, SeekOrigin.Current);
                    chunk.Layer = jonbr.ReadInt32();
                    jonbr.BaseStream.Seek(0xC, SeekOrigin.Current);
                    chunks.Add(chunk);
                }
            }
            for (var h = 0; h < hurtboxcount; h++)
            {
                var hurt = new JonbinBox();
                hurt.id = jonbr.ReadInt32();
                hurt.x = jonbr.ReadSingle();
                hurt.y = jonbr.ReadSingle();
                hurt.width = jonbr.ReadSingle();
                if (is_uni)
                {
                    hurt.width *= -1;
                }
                if (hurt.width < 0 && !is_uni)
                {
                    hurt.width *= -1;
                    is_uni = true;
                }
                hurt.height = jonbr.ReadSingle();
                hurtboxes.Add(hurt);
            }
            for (var h = 0; h < hitboxcount; h++)
            {
                var hit = new JonbinBox();
                hit.id = jonbr.ReadInt32();
                hit.x = jonbr.ReadSingle();
                hit.y = jonbr.ReadSingle();
                hit.width = jonbr.ReadSingle();
                if (is_uni)
                {
                    hit.width *= -1;
                }
                if (hit.width < 0 && !is_uni)
                {
                    hit.width *= -1;
                    is_uni = true;
                }
                hit.height = jonbr.ReadSingle();
                hitboxes.Add(hit);
            }
            jonbr.Close();
        }

        public void renderBoxes()
        {
            if (is_gg)
            {
                boxbitmap = new Bitmap(1224, 1224);
                MARGIN = 0;
            }
            else
            {
                boxbitmap = new Bitmap((int)texwidth + MARGIN, (int)texheight + MARGIN);
            }
            Pen boxpen = new Pen(HURT_COLOR);
            boxpen.Width = 2;
            int choffsetx = 640;
            int choffsety = 802;
            if (chunks.Count > 0)
            {
                choffsetx = -(int)chunks[0].DestX;
                choffsety = -(int)chunks[0].DestY;
            }
            using (Graphics g = Graphics.FromImage(boxbitmap))
            {
                if (is_uni)
                {
                    /*marginbitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    g.DrawImage(marginbitmap, pxoffsetx + MARGIN, pxoffsety + MARGIN);
                    g.ScaleTransform(-1, 1);*/
                }
                else if (!is_gg)
                {
                    g.DrawImage(marginbitmap, pxoffsetx + MARGIN, pxoffsety + MARGIN);
                }
                foreach (var box in hurtboxes)
                {
                    float tempx = box.x + choffsetx + MARGIN;
                    float tempy = box.y + choffsety + MARGIN;
                    g.DrawRectangle(boxpen, tempx, tempy, box.width, box.height);
                }
                boxpen.Color = HIT_COLOR;
                foreach (var box in hitboxes)
                {
                    float tempx = box.x + choffsetx + MARGIN;
                    float tempy = box.y + choffsety + MARGIN;
                    g.DrawRectangle(boxpen, tempx, tempy, box.width, box.height);
                }
            }
        }
    }

    public class JonbinBox
    {
        public int id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public override string ToString()
        {
            return width.ToString() + " by " + height.ToString() + " box at (" + x.ToString() + "," + y.ToString() + ")";
        }
    }
    public class JonbinChunk
    {
        public float SrcX { get; set; }
        public float SrcY { get; set; }
        public float SrcWidth { get; set; }
        public float SrcHeight { get; set; }
        public float DestX { get; set; }
        public float DestY { get; set; }
        public float DestWidth { get; set; }
        public float DestHeight { get; set; }
        public int Layer { get; set; }

    }
}
