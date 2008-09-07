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
#include "gst-video-capturer.h"
#include <stdlib.h>
#include <unistd.h>

	
GtkWidget*
create_window (void)
{
	GtkWidget *window;
	GstVideoCapturer *gvc;
	gchar *error=NULL;
	
	
   /* Create a new window */
   window = gtk_window_new (GTK_WINDOW_TOPLEVEL);
   gtk_window_set_title (GTK_WINDOW (window), "Capturer");

   /* Connect destroy event to the window. */
   gtk_signal_connect (GTK_OBJECT (window), "destroy",
                       GTK_SIGNAL_FUNC(gtk_main_quit), NULL);
	
	return window;
}


int
main (int argc, char *argv[])
{
 	GtkWidget *window;
 	GstVideoCapturer *gvc;
 	GError *error=NULL;


	gtk_init (&argc, &argv);

	/*Create GstVideoCapturer*/
	gst_video_capturer_init_backend (&argc, &argv);
	//gvc = gst_video_capturer_new (GVC_USE_TYPE_DEVICE_CAPTURE, &error );
	gvc = gst_video_capturer_new (GVC_USE_TYPE_VIDEO_TRANSCODE,"/home/ando/Lacie/polo 5 complu 1 rey.avi","/home/ando/janderrrr.avi", &error );
	//gvc = gst_video_capturer_new (GVC_USE_TYPE_TEST, &error );
	
	window = create_window ();

	gtk_container_add(GTK_CONTAINER(window),GTK_WIDGET(gvc));
	gtk_widget_show (GTK_WIDGET(gvc));
	gtk_widget_show (window);
	g_print("jander");
	
	gst_video_capturer_rec(gvc);

	gtk_main ();
	return 0;
}
