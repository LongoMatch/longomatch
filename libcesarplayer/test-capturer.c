/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * main.c
 * Copyright (C) Andoni Morales Alastruey 2008 <ylatuya@gmail.com>
 *
 * main.c is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * main.c is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <gtk/gtk.h>
#include "gst-camera-capturer.h"
#include "lgm-device.h"

static GtkWidget *recbutton, *stopbutton;
static int sargc;
static char **sargv;

static void
rec_clicked_cb (GtkButton * b, GstCameraCapturer * gcc)
{
  gst_camera_capturer_start (gcc);
}

static void
stop_clicked (GtkButton * b, GstCameraCapturer * gcc)
{
  gst_camera_capturer_stop (gcc);
}

static void
on_delete_cb (GtkWidget * video)
{
  gtk_main_quit ();
}

static void
on_realized_cb (GtkWidget * video)
{
  GstCameraCapturer *gvc;
  guintptr window;
  GError *error = NULL;

  window = lgm_get_window_handle (gtk_widget_get_window (video));

  gvc = gst_camera_capturer_new (&error);
  gst_camera_capturer_configure (gvc, sargv[1],
      (CaptureSourceType) atoi (sargv[4]), sargv[2], sargv[3],
      0, 0, 0, 0,
      VIDEO_ENCODER_H264, AUDIO_ENCODER_AAC,
      VIDEO_MUXER_MP4, 1000, 100, TRUE, 640, 480, window);
  gst_camera_capturer_run (gvc, FALSE);
  g_signal_connect (G_OBJECT (recbutton), "clicked",
      G_CALLBACK (rec_clicked_cb), gvc);
  g_signal_connect (G_OBJECT (stopbutton), "clicked",
      G_CALLBACK (stop_clicked), gvc);
}

void
create_window (void)
{
  GtkWidget *window, *vbox, *hbox, *video;

  /* Create a new window */
  window = gtk_window_new (GTK_WINDOW_TOPLEVEL);
  gtk_window_set_title (GTK_WINDOW (window), "Capturer");
  vbox = gtk_vbox_new (TRUE, 0);
  hbox = gtk_hbox_new (TRUE, 0);
  recbutton = gtk_button_new_from_stock ("gtk-rec");
  stopbutton = gtk_button_new_from_stock ("gtk-stop");
  video = gtk_drawing_area_new ();
  GTK_WIDGET_UNSET_FLAGS (video, GTK_DOUBLE_BUFFERED);

  gtk_container_add (GTK_CONTAINER (window), GTK_WIDGET (vbox));
  gtk_box_pack_start (GTK_BOX (vbox), GTK_WIDGET (video), TRUE, TRUE, 0);
  gtk_box_pack_start (GTK_BOX (vbox), GTK_WIDGET (hbox), FALSE, FALSE, 0);
  gtk_box_pack_start (GTK_BOX (hbox), recbutton, TRUE, TRUE, 0);
  gtk_box_pack_start (GTK_BOX (hbox), stopbutton, TRUE, TRUE, 0);
  g_signal_connect (video, "realize", G_CALLBACK (on_realized_cb), NULL);
  g_signal_connect (video, "delete_event", G_CALLBACK (on_delete_cb), NULL);
  gtk_widget_show_all (window);
}



int
main (int argc, char **argv)
{
  if (argc != 5) {
    g_print
        ("Usage: test-capturer output_file device_name device_id device_type \n");
    return 1;
  }
  gtk_init (&argc, &argv);
  sargc = argc;
  sargv = argv;

  /*Create GstVideoCapturer */
  lgm_init_backend (argc, argv);
  create_window ();
  gtk_main ();

  return 0;
}
