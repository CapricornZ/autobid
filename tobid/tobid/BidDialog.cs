﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace tobid
{
    public partial class BidDialog : Form
    {
        public BidDialog()
        {
            InitializeComponent();
            this.SetVisibleCore(false);
        }

        private void object2InputBox(System.Windows.Forms.TextBox textBox, rest.Position pos)
        {
            textBox.Text = pos.x + "," + pos.y;
        }

        private rest.Position inputBox2Object(System.Windows.Forms.TextBox textBox, int offsetX, int offsetY)
        {
            rest.Position pos = this.inputBox2Object(textBox);
            pos.x += offsetX;
            pos.y += offsetY;
            return pos;
        }

        private rest.Position inputBox2Object(System.Windows.Forms.TextBox textBox)
        {
            string[] pos = textBox.Text.Split(new char[] { ',' });
            return new rest.Position(Int16.Parse(pos[0]), Int16.Parse(pos[1]));
        }

        private void BidDialog_Load(object sender, EventArgs e)
        {
            if (this.bid != null)
            {
                this.object2InputBox(this.textBox1, bid.give.price);
                this.object2InputBox(this.textBox2, bid.give.inputBox);
                this.object2InputBox(this.textBox3, bid.give.button);

                this.object2InputBox(this.textBox4, bid.submit.captcha[0]);
                this.object2InputBox(this.textBox5, bid.submit.captcha[1]);
                this.object2InputBox(this.textBox6, bid.submit.inputBox);
                this.object2InputBox(this.textBox7, bid.submit.buttons[0]);
            }
        }

        public tobid.rest.Bid bid { get; set; }
        public Boolean cancel { get; set; }

        private void onOK_Click(object sender, EventArgs e)
        {   
            tobid.rest.GivePrice givePrice = new tobid.rest.GivePrice();
            givePrice.price = this.inputBox2Object(this.textBox1);//价格
            givePrice.inputBox = this.inputBox2Object(this.textBox2);//输入价格
            givePrice.button = this.inputBox2Object(this.textBox3);//出价按钮

            tobid.rest.SubmitPrice submit = new tobid.rest.SubmitPrice();
            submit.captcha = new rest.Position[]{
                this.inputBox2Object(this.textBox4),//校验码
                this.inputBox2Object(this.textBox5)//校验码提示
            };
            submit.inputBox = this.inputBox2Object(this.textBox6);//输入校验码

            string[] posBtnOK = this.textBox7.Text.Split(new char[] { ',' });
            submit.buttons = new rest.Position[]{
                this.inputBox2Object(this.textBox7),//确定按钮
                this.inputBox2Object(this.textBox7, offsetX:186, offsetY:0)//取消按钮
            };

            this.bid = new rest.Bid();
            this.bid.give = givePrice;
            this.bid.submit = submit;

            this.cancel = false;
            this.Close();
        }

        private void onCANCEL_Click(object sender, EventArgs e)
        {
            this.cancel = true;
            this.Close();
        }

    }
}