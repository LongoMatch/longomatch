/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * gtk-foobar
 * Copyright (C)  2008 <>
 * 
 * gtk-foobar is free software.
 * 
 * You may redistribute it and/or modify it under the terms of the
 * GNU General Public License, as published by the Free Software
 * Foundation; either version 2 of the License, or (at your option)
 * any later version.
 * 
 * gtk-foobar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with gtk-foobar.  If not, write to:
 * 	The Free Software Foundation, Inc.,
 * 	51 Franklin Street, Fifth Floor
 * 	Boston, MA  02110-1301, USA.
 */

#ifndef _GTK_TIMESCALE_H_
#define _GTK_TIMESCALE_H_

#if BUILDING_DLL
# define DLLIMPORT __declspec (dllexport)
#else /* Not BUILDING_DLL */
# define DLLIMPORT __declspec (dllimport)
#endif /* Not BUILDING_DLL */

#include <glib-object.h>
#include <gtk/gtk.h>
#include "gtkenhancedscale.h"

G_BEGIN_DECLS

#define GTK_TYPE_TIMESCALE             (gtk_timescale_get_type ())
#define GTK_TIMESCALE(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GTK_TYPE_TIMESCALE, GtkTimescale))
#define GTK_TIMESCALE_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GTK_TYPE_TIMESCALE, GtkTimescaleClass))
#define GTK_IS_TIMESCALE(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GTK_TYPE_TIMESCALE))
#define GTK_IS_TIMESCALE_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GTK_TYPE_TIMESCALE))
#define GTK_TIMESCALE_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GTK_TYPE_TIMESCALE, GtkTimescaleClass))

typedef struct _GtkTimescaleClass GtkTimescaleClass;
typedef struct _GtkTimescale GtkTimescale;
typedef struct _GtkTimescalePrivate GtkTimescalePrivate;

struct _GtkTimescaleClass
{
	GtkHBoxClass parent_class;
	
	void (*pos_changed) (GtkTimescale *gts, double val);
	void (*in_changed)  (GtkTimescale *gts, double val);
	void (*out_changed) (GtkTimescale *gts, double val);
};

struct _GtkTimescale
{
	GtkHBox hbox;
	GtkEnhancedScale scale;
	
	GtkAdjustment * adjustment[3];
	int num_adjustment;
	gint *handler_id;
	
};

GType gtk_timescale_get_type (void) G_GNUC_CONST;
GtkWidget *gtk_timescale_new(gint32 upper);
void gtk_timescale_set_bounds(GtkTimescale *gts,gdouble lower, gdouble upper);
void gtk_timescale_adjust_position(GtkTimescale *gts,gdouble val, gint adj); 
void gtk_timescale_set_segment(GtkTimescale *gts, gdouble in, gdouble out);


G_END_DECLS

#endif /* _GTK_TIMESCALE_H_ */
