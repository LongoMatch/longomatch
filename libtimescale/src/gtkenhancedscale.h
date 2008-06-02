/* GtkEnhancedScale - A gtk(h)scale with multiple sliders
* Copyright (C) 2001 - Ronald Bultje
* Copyright (C) 2001-2007 Dan Dennedy <dan@dennedy.org>
* 
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, 
* USA.
*/

/* Some code used was taken from GtkScale and GtkRange, all part
 * of the Gimp Toolkit (Gtk+), http://www.gtk.org/
 */

#ifndef __GTK_ENHANCED_SCALE_H__
#define __GTK_ENHANCED_SCALE_H__

#if BUILDING_DLL
# define DLLIMPORT __declspec (dllexport)
#else /* Not BUILDING_DLL */
# define DLLIMPORT __declspec (dllimport)
#endif /* Not BUILDING_DLL */

#include <gdk/gdk.h>
#include <gtk/gtkwidget.h>

#ifdef __cplusplus
extern "C"
{
#endif /* __cplusplus */

#define GTK_ENHANCED_SCALE(obj)          GTK_CHECK_CAST (obj, gtk_enhanced_scale_get_type (), GtkEnhancedScale)
#define GTK_ENHANCED_SCALE_CLASS(klass)  GTK_CHECK_CLASS_CAST (klass, gtk_enhanced_scale_get_type (), GtkEnhancedScaleClass)
#define GTK_IS_ENHANCED_SCALE(obj)       GTK_CHECK_TYPE (obj, gtk_enhanced_scale_get_type ())

typedef struct _GtkEnhancedScale GtkEnhancedScale;
typedef struct _GtkEnhancedScaleClass GtkEnhancedScaleClass;

struct _GtkEnhancedScale
{
	GtkWidget widget;

	GdkWindow *trough;
	GdkWindow **slider;

	GtkObject **adjustment;
	gint num_adjustments;
	gint *handler_id;

	guint in_child :3;
	guint click_child :3;

	gint active_slider;
	gint clicked_slider;
	gint x_click_point;
	GArray *breaks;
};

struct _GtkEnhancedScaleClass
{
	GtkWidgetClass parent_class;

	gint slider_width;
	gint trough_width;
	gint stepper_size;
	gint stepper_slider_spacing;
	gint arrow_width;
	gint arrow_height;

	guint8 trough;
	guint8 slider;
};

GtkWidget* gtk_enhanced_scale_new ( GtkObject * adjustment[],
									gint num_adjustments );
GType gtk_enhanced_scale_get_type ( void );
void gtk_enhanced_scale_set_breaks( GtkWidget * widget, GArray * breaks );

#ifdef __cplusplus
}
#endif /* __cplusplus */


#endif /* __GTK_ENHANCED_SCALE_H__ */
