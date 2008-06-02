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

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#include <gtk/gtkmain.h>
#include <gtk/gtksignal.h>
#include <gdk/gdkkeysyms.h>
#include <gtk/gtkadjustment.h>
#include "gtkenhancedscale.h"
#include <string.h>
#include <stdio.h>
#include <malloc.h>
#include <math.h>

#undef FANCY
#define ENHANCED_SCALE_CLASS(w)  GTK_ENHANCED_SCALE_CLASS ( G_OBJECT_GET_CLASS( G_OBJECT( w )  ) ) 
// #define DEBUG

static void gtk_enhanced_scale_class_init ( GtkEnhancedScaleClass *klass );
static void gtk_enhanced_scale_init ( GtkEnhancedScale *enhanced_scale );
static void gtk_enhanced_scale_destroy ( GtkObject *object );
static void gtk_enhanced_scale_realize ( GtkWidget *widget );
static void gtk_enhanced_scale_unrealize ( GtkWidget *widget );
static void gtk_enhanced_scale_size_request ( GtkWidget *widget, GtkRequisition *requisition );
static void gtk_enhanced_scale_size_allocate ( GtkWidget *widget, GtkAllocation *allocation );
static gint gtk_enhanced_scale_expose ( GtkWidget *widget, GdkEventExpose *event );
static void gtk_enhanced_scale_pos_trough ( GtkEnhancedScale *enhanced_scale, gint *x, gint *y, gint *w, gint *h );
static void gtk_enhanced_scale_draw ( GtkWidget *widget, GdkRectangle *area );
static void gtk_enhanced_scale_draw_slider ( GtkEnhancedScale *enhanced_scale, gint i );
static void gtk_enhanced_scale_draw_trough ( GtkEnhancedScale *enhanced_scale );
static void gtk_enhanced_scale_draw_selected_area (GtkEnhancedScale *enhanced_scale);
static void gtk_enhanced_scale_slider_update ( GtkEnhancedScale *enhanced_scale, gint i );
static void gtk_enhanced_scale_trough_hdims ( GtkEnhancedScale *enhanced_scale, gint *left, gint *right, gint i );
static void gtk_enhanced_scale_get_slider_position( GtkEnhancedScale *enhanced_scale, gint *x, gint *y, gint i );
static gint gtk_enhanced_scale_get_trough_border( GtkWidget *widget );
static gint gtk_enhanced_scale_get_slider_width( GtkWidget *widget );
static gint gtk_enhanced_scale_button_press( GtkWidget *widget, GdkEventButton *event );
static gint gtk_enhanced_scale_button_release( GtkWidget *widget, GdkEventButton *event );
static gint gtk_enhanced_scale_motion_notify( GtkWidget *widget, GdkEventMotion *event );
static void gtk_enhanced_scale_motion_x( GtkEnhancedScale *enhanced_scale, gint x, gint i );
static gint gtk_enhanced_scale_key_press( GtkWidget *widget, GdkEventKey *event );
static gint gtk_enhanced_scale_enter_notify( GtkWidget *widget, GdkEventCrossing *event );
static gint gtk_enhanced_scale_leave_notify( GtkWidget *widget, GdkEventCrossing *event );
static gint gtk_enhanced_scale_focus_in( GtkWidget *widget, GdkEventFocus *event );
static gint gtk_enhanced_scale_focus_out( GtkWidget *widget, GdkEventFocus *event );
static void gtk_enhanced_scale_style_set( GtkWidget *widget, GtkStyle *style );
static void gtk_enhanced_scale_adjustment_value_changed( GtkAdjustment *adjustment, gpointer data );

static GtkWidgetClass *parent_class = NULL;

/* ================================================================= */

GType gtk_enhanced_scale_get_type ()
{
	static GType enhanced_scale_type = 0;

	if ( !enhanced_scale_type )
	{
		GtkTypeInfo enhanced_scale_info =
		    {
		        "GtkEnhancedScale",
		        sizeof ( GtkEnhancedScale ),
		        sizeof ( GtkEnhancedScaleClass ),
		        ( GtkClassInitFunc ) gtk_enhanced_scale_class_init,
		        ( GtkObjectInitFunc ) gtk_enhanced_scale_init,
		        NULL,
		        NULL,
		    };
		enhanced_scale_type = gtk_type_unique ( gtk_widget_get_type (),
		                                        &enhanced_scale_info );
	}
	return enhanced_scale_type;
}

static void gtk_enhanced_scale_class_init ( GtkEnhancedScaleClass *class )
{
	GtkObjectClass *object_class;
	GtkWidgetClass *widget_class;

	object_class = ( GtkObjectClass* ) class;
	widget_class = ( GtkWidgetClass* ) class;
	parent_class = gtk_type_class ( gtk_widget_get_type () );

	object_class->destroy = gtk_enhanced_scale_destroy;

	widget_class->realize = gtk_enhanced_scale_realize;	
	widget_class->unrealize = gtk_enhanced_scale_unrealize;
	widget_class->expose_event = gtk_enhanced_scale_expose;
	widget_class->size_request = gtk_enhanced_scale_size_request;
	widget_class->size_allocate = gtk_enhanced_scale_size_allocate;

	/* Events and their corresponding reaction-functions */
	widget_class->button_press_event = gtk_enhanced_scale_button_press;
	widget_class->button_release_event = gtk_enhanced_scale_button_release;
	widget_class->motion_notify_event = gtk_enhanced_scale_motion_notify;
	widget_class->key_press_event = gtk_enhanced_scale_key_press;
	widget_class->enter_notify_event = gtk_enhanced_scale_enter_notify;
	widget_class->leave_notify_event = gtk_enhanced_scale_leave_notify;
	widget_class->focus_in_event = gtk_enhanced_scale_focus_in;
	widget_class->focus_out_event = gtk_enhanced_scale_focus_out;
	widget_class->style_set = gtk_enhanced_scale_style_set;

	class->slider_width = 32;
	class->trough_width = 10;
	class->trough = 1;
	class->slider = 2;
	class->arrow_width = 20;
	class->arrow_height = 10;
}

static void gtk_enhanced_scale_init ( GtkEnhancedScale *enhanced_scale )
{
	enhanced_scale->in_child = 0;
	enhanced_scale->click_child = 0;
	enhanced_scale->active_slider = 0; /* default */
	enhanced_scale->clicked_slider = -1; /* none */
	enhanced_scale->x_click_point = 0;
	enhanced_scale->breaks = NULL;

	GTK_WIDGET_SET_FLAGS ( GTK_WIDGET( enhanced_scale ), GTK_CAN_FOCUS );
}


GtkWidget* gtk_enhanced_scale_new1()
{
	GtkAdjustment *adjustment[ 3 ];
	GtkEnhancedScale * enhanced_scale;
	int i;
	

	
 	adjustment[ 0 ] = GTK_ADJUSTMENT(
	                               gtk_adjustment_new( 0, 0, 3000, 1, 10, 0 ) );
 	adjustment[ 1 ] = GTK_ADJUSTMENT(
	                               gtk_adjustment_new( 1000, 0, 3000, 1, 10, 0 ) );
 	adjustment[ 2 ] = GTK_ADJUSTMENT(
	                               gtk_adjustment_new( 2000, 0, 3000, 1, 10, 0 ) );
	                               
	enhanced_scale = gtk_type_new ( gtk_enhanced_scale_get_type () );
	enhanced_scale->adjustment = (GtkObject**) adjustment;
	enhanced_scale->num_adjustments = 3;
	
	enhanced_scale->handler_id = malloc( enhanced_scale->num_adjustments * sizeof(*enhanced_scale->handler_id) );
    for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
	{
		enhanced_scale->handler_id[ i ] = g_signal_connect ( adjustment[ i ], "value_changed",
		                                  G_CALLBACK( gtk_enhanced_scale_adjustment_value_changed ),
		                                  ( gpointer ) enhanced_scale );

	   
	}

	return GTK_WIDGET ( enhanced_scale );
	
}
GtkWidget* gtk_enhanced_scale_new ( GtkObject *adjustment[],
                                    gint num_adjustments )
{
	GtkEnhancedScale * enhanced_scale;
	int i;

	for ( i = 0;i < num_adjustments;i++ )
	{
		if ( adjustment[ i ] == NULL )
			return NULL;
	}

	enhanced_scale = gtk_type_new ( gtk_enhanced_scale_get_type () );
	enhanced_scale->adjustment = adjustment;
	enhanced_scale->num_adjustments = num_adjustments;

	enhanced_scale->handler_id = malloc( num_adjustments * sizeof(*enhanced_scale->handler_id) );

	for ( i = 0;i < num_adjustments;i++ )
	{
		enhanced_scale->handler_id[ i ] = g_signal_connect ( adjustment[ i ], "value_changed",
		                                  G_CALLBACK( gtk_enhanced_scale_adjustment_value_changed ),
		                                  ( gpointer ) enhanced_scale );
	}

	return GTK_WIDGET ( enhanced_scale );
}

static void gtk_enhanced_scale_destroy ( GtkObject *object )
{
	GtkEnhancedScale * enhanced_scale;
	gint i;

	g_return_if_fail ( object != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( object ) );

	enhanced_scale = GTK_ENHANCED_SCALE ( object );
#ifdef DEBUG

	printf( "Received destroy signal, let's disappear!\n" );
#endif
    
     //El handler se elimina solo al finalizar el objeto  
	/*for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
		gtk_signal_disconnect( enhanced_scale->adjustment[ i ], enhanced_scale->handler_id[ i ] );*/

	if ( enhanced_scale->breaks != NULL )
		g_array_free( enhanced_scale->breaks, TRUE );

	if ( GTK_OBJECT_CLASS ( parent_class ) ->destroy )
		( * GTK_OBJECT_CLASS ( parent_class ) ->destroy ) ( object );
#ifdef DEBUG

	printf( "Finished destroy signal\n" );
#endif
}

static void gtk_enhanced_scale_realize ( GtkWidget *widget )
{
	GdkWindowAttr attributes;
	GtkEnhancedScale *enhanced_scale;
	gint attributes_mask;
	gint x = 0, y = 0, w = 0, h = 0;
	gint slider_width;
	gint i;

	g_return_if_fail ( widget != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ) );

	slider_width = gtk_enhanced_scale_get_slider_width( widget );

	GTK_WIDGET_SET_FLAGS ( widget, GTK_REALIZED );
	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	attributes.x = widget->allocation.x;
	attributes.y = widget->allocation.y;
	attributes.width = widget->allocation.width;
	attributes.height = widget->allocation.height;
	attributes.wclass = GDK_INPUT_OUTPUT;
	attributes.window_type = GDK_WINDOW_CHILD;
	attributes.event_mask = gtk_widget_get_events ( widget ) |
	                        GDK_EXPOSURE_MASK | GDK_BUTTON_PRESS_MASK |
	                        GDK_BUTTON_RELEASE_MASK | GDK_POINTER_MOTION_MASK |
	                        GDK_POINTER_MOTION_HINT_MASK;
	attributes.visual = gtk_widget_get_visual ( widget );
	attributes.colormap = gtk_widget_get_colormap ( widget );
	attributes_mask = GDK_WA_X | GDK_WA_Y | GDK_WA_VISUAL | GDK_WA_COLORMAP;
	widget->window = gdk_window_new ( widget->parent->window, &attributes, attributes_mask );

	gtk_enhanced_scale_pos_trough ( enhanced_scale, &x, &y, &w, &h );
	attributes.x = x;
	attributes.y = y;
	attributes.width = w;
	attributes.height = h;
	attributes.wclass = GDK_INPUT_OUTPUT;
	attributes.window_type = GDK_WINDOW_CHILD;
	attributes.event_mask = gtk_widget_get_events ( widget ) |
	                        ( GDK_EXPOSURE_MASK |
	                          GDK_BUTTON_PRESS_MASK |
	                          GDK_BUTTON_RELEASE_MASK |
	                          GDK_ENTER_NOTIFY_MASK |
	                          GDK_LEAVE_NOTIFY_MASK );
	attributes.visual = gtk_widget_get_visual ( widget );
	attributes.colormap = gtk_widget_get_colormap ( widget );
	attributes_mask = GDK_WA_X | GDK_WA_Y | GDK_WA_VISUAL | GDK_WA_COLORMAP;
#ifdef DEBUG

	printf( "Scale trough properties - x: %d, y: %d, w: %d, h: %d\n", x, y, w, h );
#endif

	enhanced_scale->trough = gdk_window_new ( widget->window, &attributes, attributes_mask );

	enhanced_scale->slider = malloc( enhanced_scale->num_adjustments * sizeof(*enhanced_scale->handler_id) );
	attributes.width = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width;
	attributes.height = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height;
	attributes.event_mask |= ( GDK_BUTTON_MOTION_MASK |
	                           GDK_POINTER_MOTION_HINT_MASK );
	enhanced_scale->slider[ 0 ] = gdk_window_new ( enhanced_scale->trough,
	                              &attributes, attributes_mask );
	attributes.y += ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough_width - 1;
	attributes.width = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2;
	for ( i = 1;i < enhanced_scale->num_adjustments;i++ )
		enhanced_scale->slider[ i ] = gdk_window_new ( enhanced_scale->trough,
		                              &attributes, attributes_mask );

	widget->style = gtk_style_attach ( widget->style, widget->window );


	for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
		gdk_window_set_user_data ( enhanced_scale->slider[ i ], widget );
	gdk_window_set_user_data ( enhanced_scale->trough, widget );
	gdk_window_set_user_data ( widget->window, widget );

	for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
		gtk_style_set_background ( widget->style, enhanced_scale->slider[ i ], GTK_STATE_NORMAL );
	gtk_style_set_background ( widget->style, enhanced_scale->trough, GTK_STATE_NORMAL );
	gtk_style_set_background ( widget->style, widget->window, GTK_STATE_NORMAL );

	for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
	{
		gtk_enhanced_scale_slider_update ( GTK_ENHANCED_SCALE ( widget ), i );
		gdk_window_show ( enhanced_scale->slider[ i ] );
	}
	gdk_window_show ( enhanced_scale->trough );
}

static void gtk_enhanced_scale_unrealize ( GtkWidget *widget )
{
	GtkEnhancedScale * enhanced_scale;
	int i;

	g_return_if_fail ( widget != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ) );

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
	{
		if ( enhanced_scale->slider[ i ] )
		{
			gdk_window_set_user_data ( enhanced_scale->slider[ i ], NULL );
			gdk_window_destroy ( enhanced_scale->slider[ i ] );
			enhanced_scale->slider[ i ] = NULL;
		}
	}
	if ( enhanced_scale->trough )
	{
		gdk_window_set_user_data ( enhanced_scale->trough, NULL );
		gdk_window_destroy ( enhanced_scale->trough );
		enhanced_scale->trough = NULL;
	}

	gdk_window_set_user_data ( widget->window, NULL );
#ifdef DEBUG

	printf( "Received unrealize signal, let's disappear!\n" );
#endif

	if ( GTK_WIDGET_CLASS ( parent_class ) ->unrealize )
		( * GTK_WIDGET_CLASS ( parent_class ) ->unrealize ) ( widget );
}

static void gtk_enhanced_scale_size_request ( GtkWidget *widget,
        GtkRequisition *requisition )
{
	GtkEnhancedScale * enhanced_scale;
	gint trough_border, slider_width;

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

#ifdef DEBUG

	printf( "Received size_request signal\n" );
#endif

	trough_border = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );
	slider_width = gtk_enhanced_scale_get_slider_width( GTK_WIDGET( enhanced_scale ) );

	requisition->width = ( ENHANCED_SCALE_CLASS( enhanced_scale ) ->arrow_width + trough_border ) * 4;
	requisition->height = ( slider_width + trough_border * 2 );

}

static void gtk_enhanced_scale_size_allocate ( GtkWidget *widget,
        GtkAllocation *allocation )
{
	GtkEnhancedScale * enhanced_scale;

	g_return_if_fail ( widget != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ) );
	g_return_if_fail ( allocation != NULL );

	widget->allocation = *allocation;
	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

#ifdef DEBUG

	printf( "Received size allocate signal: %d\n", allocation->width );
#endif

	if ( GTK_WIDGET_REALIZED ( widget ) )
	{
		gint i, x, y, width, height;

		gdk_window_move_resize ( widget->window,
		                         allocation->x, allocation->y,
		                         allocation->width, allocation->height );

		x = allocation->x;
		y = allocation->y;
		width = allocation->width;
		height = allocation->height;
		gtk_enhanced_scale_pos_trough( enhanced_scale, &x, &y, &width, &height );
		gdk_window_move_resize ( enhanced_scale->trough, x, y, width, height );

		for ( i = 0; i < enhanced_scale->num_adjustments; i++ )
			gtk_enhanced_scale_slider_update( enhanced_scale, i );

	}

}

static gint gtk_enhanced_scale_expose ( GtkWidget *widget, GdkEventExpose *event )
{
	GtkEnhancedScale * enhanced_scale;
	GdkRectangle area;

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );
	g_return_val_if_fail ( widget->window != NULL, FALSE );

	if ( event->count > 0 )
		return FALSE;

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	gdk_window_clear_area ( widget->window, 0, 0,
	                        widget->allocation.width, widget->allocation.height );

	/* This would be the place to draw it */
	area.x = 0;
	area.y = 0;
	area.width = widget->allocation.width;
	area.height = widget->allocation.height;
	gtk_enhanced_scale_draw ( widget, &area );

	return FALSE;
}

/* =========================================================================
 * Paint functions
 * Some were directly taken from GtkScale, GtkHScale and GtkRange
 */

static void gtk_enhanced_scale_draw ( GtkWidget *widget, GdkRectangle *area )
{
	GtkEnhancedScale * enhanced_scale;
	GdkRectangle tmp_area;
	gint x = 0, y = 0, w = 0, h = 0;

	g_return_if_fail ( widget != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ) );
#ifdef DEBUG

	printf( "Trying to draw enhanced_scale\n" );
#endif

	if ( GTK_WIDGET_VISIBLE ( widget ) && GTK_WIDGET_MAPPED ( widget ) )
	{
		gint i;
#ifdef DEBUG

		printf( "Drawing enhanced_scale\n" );
#endif

		enhanced_scale = GTK_ENHANCED_SCALE ( widget );

		gtk_enhanced_scale_pos_trough ( GTK_ENHANCED_SCALE ( widget ), &x, &y, &w, &h );

		tmp_area.x = x;
		tmp_area.y = y;
		tmp_area.width = w;
		tmp_area.height = h;

#ifdef DEBUG

		printf( "Drawing enhanced_scale with size = %d\n", w );
#endif
		//if (gdk_rectangle_intersect (area, &tmp_area, area))
		{
			for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
			{
				gtk_enhanced_scale_slider_update ( enhanced_scale, i );
				gtk_enhanced_scale_draw_slider ( enhanced_scale, i );
			}
			gtk_enhanced_scale_draw_trough ( enhanced_scale );
		}
	}
}

static void gtk_enhanced_scale_draw_trough ( GtkEnhancedScale *enhanced_scale )
{
	g_return_if_fail ( enhanced_scale != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE( enhanced_scale ) );

	if ( enhanced_scale->trough )
	{
		gint x, x2, y, b;
		gint left, right;

#ifdef DEBUG

		printf( "Drawing trough: %d\n", GTK_WIDGET( enhanced_scale ) ->allocation.width - 2 );
#endif

		if ( enhanced_scale->num_adjustments > 1 )
		{
			/* trough */
			b = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );
			gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right, 0 );

#ifdef FANCY
			gtk_paint_box (GTK_WIDGET (enhanced_scale)->style,
                                 enhanced_scale->trough, GTK_STATE_NORMAL,
                                 GTK_SHADOW_IN, NULL, GTK_WIDGET(enhanced_scale),
                                 "trough", ENHANCED_SCALE_CLASS (enhanced_scale)->arrow_width/2,
                                 ENHANCED_SCALE_CLASS (enhanced_scale)->arrow_height + b, right-left + 4,
                                 ENHANCED_SCALE_CLASS (enhanced_scale)->trough_width);
#endif
                            
			gdk_draw_rectangle( enhanced_scale->trough, GTK_WIDGET ( enhanced_scale ) ->style->mid_gc[ GTK_STATE_NORMAL ],
			                    TRUE, ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 + 1,
			                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + b + 1,
			                    right - left + 1,
			                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough_width - 2
			                  );

			/* selected area */
			gtk_enhanced_scale_get_slider_position( enhanced_scale, &x, &y, 1 );
			gtk_enhanced_scale_get_slider_position( enhanced_scale, &x2, &y, 2 );
			if ( x2 - x - ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 > 0 )
			{
				gdk_draw_rectangle( enhanced_scale->trough,
				                    GTK_WIDGET ( enhanced_scale ) ->style->mid_gc[ GTK_STATE_SELECTED ],
				                    TRUE,
				                    x + ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2,
				                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + b + 1,
				                    x2 - x - ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2,
				                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough_width - 2
				                  );
			}

		}
		else
		{
			/* trough */
			b = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );
			gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right, 0 );

#ifdef FANCY
			gtk_paint_box (GTK_WIDGET (enhanced_scale)->style,
                                 enhanced_scale->trough, GTK_STATE_NORMAL,
                                 GTK_SHADOW_IN, NULL, GTK_WIDGET(enhanced_scale),
                                 "trough", ENHANCED_SCALE_CLASS (enhanced_scale)->arrow_width/2,
                                 ENHANCED_SCALE_CLASS (enhanced_scale)->arrow_height + b, right-left + 4,
                                 ENHANCED_SCALE_CLASS (enhanced_scale)->trough_width);
#endif
                                 
			gdk_draw_rectangle( enhanced_scale->trough, GTK_WIDGET ( enhanced_scale ) ->style->mid_gc[ GTK_STATE_NORMAL ],
			                    TRUE, ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 + 1,
			                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + b + 1,
			                    right - left + 1,
			                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough_width - 2
			                  );
		}

		/* scene breaks */
		if ( enhanced_scale->breaks != NULL )
			if ( enhanced_scale->breaks->len > 1 )
				for ( x2 = 0; x2 < ( enhanced_scale->breaks->len - 1 ); x2++ )
				{
					gint left, right;

					gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right, 0 );
					x = left + ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2;
					x += ( ( right - left ) * ( g_array_index( enhanced_scale->breaks, gint, x2 ) -
					                            GTK_ADJUSTMENT( enhanced_scale->adjustment[ 0 ] ) ->lower ) /
					       ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ 0 ] ) ->upper -
					         GTK_ADJUSTMENT( enhanced_scale->adjustment[ 0 ] ) ->lower -
					         GTK_ADJUSTMENT( enhanced_scale->adjustment[ 0 ] ) ->page_size ) );
					gdk_draw_line ( enhanced_scale->trough, GTK_WIDGET ( enhanced_scale ) ->style->light_gc[ GTK_STATE_NORMAL ],
					                x,
					                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + b,
					                x,
					                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + b + ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough_width - 1
					              );

				}
#ifdef FANCY
		/* frame position */
		gtk_enhanced_scale_get_slider_position( enhanced_scale, &x, &y, 0 );
		gtk_paint_box (GTK_WIDGET (enhanced_scale)->style,
                         enhanced_scale->trough, GTK_STATE_NORMAL,
                         GTK_SHADOW_OUT, NULL, GTK_WIDGET(enhanced_scale),
                         NULL,
                         x + ENHANCED_SCALE_CLASS (enhanced_scale)->arrow_width/2 - 1,
                         ENHANCED_SCALE_CLASS (enhanced_scale)->arrow_height + b - 1,
                         3,
                         ENHANCED_SCALE_CLASS (enhanced_scale)->trough_width);
#endif
	}
}

static void gtk_enhanced_scale_draw_selected_area (GtkEnhancedScale *enhanced_scale){
	
	gint x, x2, y,b;
				b = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );
	        gtk_enhanced_scale_get_slider_position( enhanced_scale, &x, &y, 1 );
			gtk_enhanced_scale_get_slider_position( enhanced_scale, &x2, &y, 2 );
			if ( x2 - x - ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 > 0 )
			{
				gdk_draw_rectangle( enhanced_scale->trough,
				                    GTK_WIDGET ( enhanced_scale ) ->style->mid_gc[ GTK_STATE_SELECTED ],
				                    TRUE,
				                    x + ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2,
				                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height + b + 1,
				                    x2 - x - ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2,
				                    ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough_width - 2
				                  );
			}
}

static void gtk_enhanced_scale_draw_slider ( GtkEnhancedScale *enhanced_scale, gint i )
{
	GtkStateType state_type;
	GdkPoint points[ 3 ];

	g_return_if_fail ( enhanced_scale != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( enhanced_scale ) );
	g_return_if_fail ( i >= 0 );
	g_return_if_fail ( i < enhanced_scale->num_adjustments );

	if ( enhanced_scale->slider[ i ] )
	{
#ifdef DEBUG
		printf( "Drawing slider %d\n", i );
#endif

		if ( ( enhanced_scale->in_child == ENHANCED_SCALE_CLASS ( enhanced_scale ) ->slider ) ||
		        ( enhanced_scale->click_child == ENHANCED_SCALE_CLASS ( enhanced_scale ) ->slider ) )
			state_type = GTK_STATE_SELECTED;
		else
			state_type = GTK_STATE_SELECTED;

		switch ( i )
		{
		case 0:
			gdk_draw_line ( enhanced_scale->slider[ i ], GTK_WIDGET ( enhanced_scale ) ->style->dark_gc[ GTK_STATE_NORMAL ],
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width,
			                0,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 + 1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1
			              );
			gdk_draw_line ( enhanced_scale->slider[ i ], GTK_WIDGET ( enhanced_scale ) ->style->dark_gc[ GTK_STATE_NORMAL ],
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width + 1,
			                1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 + 2,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height
			              );
			points[ 0 ].x = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width;
			points[ 0 ].y = 0;
			points[ 1 ].x = 0;
			points[ 1 ].y = 0;
			points[ 2 ].x = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2;
			points[ 2 ].y = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height;
			break;
		case 1:
			gdk_draw_line ( enhanced_scale->slider[ i ], GTK_WIDGET ( enhanced_scale ) ->style->dark_gc[ GTK_STATE_NORMAL ],
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1,
			                2,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1
			              );
			gdk_draw_line ( enhanced_scale->slider[ i ], GTK_WIDGET ( enhanced_scale ) ->style->dark_gc[ GTK_STATE_NORMAL ],
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1,
			                2,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1
			              );
			points[ 0 ].x = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1;
			points[ 0 ].y = 0;
			points[ 1 ].x = 0;
			points[ 1 ].y = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1;
			points[ 2 ].x = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1;
			points[ 2 ].y = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1;
			break;
		case 2:
			gdk_draw_line ( enhanced_scale->slider[ i ], GTK_WIDGET ( enhanced_scale ) ->style->dark_gc[ GTK_STATE_NORMAL ],
			                2,
			                2,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1
			              );
			gdk_draw_line ( enhanced_scale->slider[ i ], GTK_WIDGET ( enhanced_scale ) ->style->dark_gc[ GTK_STATE_NORMAL ],
			                1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2,
			                ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1
			              );
			points[ 0 ].x = 0;
			points[ 0 ].y = 0;
			points[ 1 ].x = 0;
			points[ 1 ].y = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1;
			points[ 2 ].x = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_width / 2 - 1;
			points[ 2 ].y = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->arrow_height - 1;
			break;
		default:
			return ;
		}
		gdk_draw_polygon( enhanced_scale->slider[ i ],
		                  GTK_WIDGET( enhanced_scale ) ->style->bg_gc[ state_type ], TRUE, points, 3 );
	}
}

/* =========================================================================
 * Functions to make life easier
 * positioning functions etc, mostly directly taken from GtkScale, GtkHScale
 * and GtkRange
 */

static void gtk_enhanced_scale_pos_trough ( GtkEnhancedScale *enhanced_scale,
        gint *x, gint *y, gint *w, gint *h )
{
	GtkWidget * widget;
	gint trough_border, slider_width;

	g_return_if_fail ( enhanced_scale != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( enhanced_scale ) );
	g_return_if_fail ( ( x != NULL ) && ( y != NULL ) && ( w != NULL ) && ( h != NULL ) );

	widget = GTK_WIDGET ( enhanced_scale );
	trough_border = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );
	slider_width = gtk_enhanced_scale_get_slider_width( GTK_WIDGET( enhanced_scale ) );

	*w = widget->allocation.width - 2;
	*h = ( slider_width + trough_border * 2 );
	*x = 1;
	*y = ( widget->allocation.height - *h ) / 2;
}

static void gtk_enhanced_scale_slider_update ( GtkEnhancedScale *enhanced_scale, gint i )
{
	/* i is the number of the adjustment */
	gint left;
	gint right;
	gint x;
	gint trough_border;

	g_return_if_fail ( enhanced_scale != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( enhanced_scale ) );
	g_return_if_fail ( i >= 0 );
	g_return_if_fail ( i < enhanced_scale->num_adjustments );

	trough_border = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );

	if ( GTK_WIDGET_REALIZED ( enhanced_scale ) )
	{
		gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right, i );
		x = left;
#ifdef DEBUG

		printf( "Updating slider %d\n", i );
#endif

		// Lower value
		if ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value <
		        GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower )
		{
			GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value =
			    GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower;
			g_signal_emit_by_name ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ), "value_changed" );
		}
		
		// Higher value
		else if ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value >
		          GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->upper )
		{
			GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value =
			    GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->upper;
			g_signal_emit_by_name( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ), "value_changed" );
		}
		
		// Adjustable value
		if ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower !=
		        ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->upper -
		          GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->page_size ) )
		{
			x += ( ( right - left ) * ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value -
			                            GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower ) /
			       ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->upper -
			         GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower -
			         GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->page_size ) );
		}
		/*
		if (GTK_ADJUSTMENT(enhanced_scale->adjustment[i])->lower !=
			(GTK_ADJUSTMENT(enhanced_scale->adjustment[i])->upper))
		{
			x += ((right - left) * (GTK_ADJUSTMENT(enhanced_scale->adjustment[i])->value -
				GTK_ADJUSTMENT(enhanced_scale->adjustment[i])->lower) /
				(GTK_ADJUSTMENT(enhanced_scale->adjustment[i])->upper -
				GTK_ADJUSTMENT(enhanced_scale->adjustment[i])->lower));
		}
		*/
		if ( x < left )
			x = left;
		else if ( x > right )
			x = right;

		gdk_window_move ( enhanced_scale->slider[ i ], x,
		                  trough_border + ( i > 0 ? ENHANCED_SCALE_CLASS( enhanced_scale ) ->arrow_height + ENHANCED_SCALE_CLASS( enhanced_scale ) ->trough_width - 1 : 0 ) );
	}
}

static void gtk_enhanced_scale_trough_hdims ( GtkEnhancedScale *enhanced_scale, gint *left, gint *right, gint i )
{
	gint trough_width;
	gint slider_length;
	gint tleft;
	gint tright;
	gint trough_border;

	g_return_if_fail ( enhanced_scale != NULL );

	gdk_window_get_size ( enhanced_scale->trough, &trough_width, NULL );
	gdk_window_get_size ( enhanced_scale->slider[ 0 ], &slider_length, NULL );

	trough_border = gtk_enhanced_scale_get_trough_border( GTK_WIDGET( enhanced_scale ) );

	tleft = trough_border;
	tright = trough_width - slider_length + trough_border;
	if ( i == 2 )
	{
		tleft += slider_length / 2;
		tright += slider_length / 2;
	}

	if ( left )
		* left = tleft;
	if ( right )
		* right = tright;
}

static void gtk_enhanced_scale_get_slider_position( GtkEnhancedScale *enhanced_scale, gint *x, gint *y, gint i )
{
	g_return_if_fail ( enhanced_scale != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( enhanced_scale ) );
	g_return_if_fail ( i >= 0 );
	g_return_if_fail ( i < enhanced_scale->num_adjustments );

	gdk_window_get_position( enhanced_scale->slider[ i ], x, y );
}

static gint gtk_enhanced_scale_get_trough_border( GtkWidget *widget )
{
	/*return gtk_style_get_prop_experimental (widget->style,
		"GtkEnhancedScale::trough_border",
		widget->style->klass->xthickness);*/ 
	return widget->style->ythickness;
}

static gint gtk_enhanced_scale_get_slider_width( GtkWidget *widget )
{
	/*return gtk_style_get_prop_experimental (widget->style,
		"GtkEnhancedScale::slider_width",
		ENHANCED_SCALE_CLASS (widget)->slider_width);*/ 
	return ENHANCED_SCALE_CLASS ( widget ) ->slider_width;
}

/* =========================================================================
 * Signal handlers/Callbacks for motion, buttons, keys, focus etc.
 * Partly helped by code from GtkScale/GtkRange
 */

static gint gtk_enhanced_scale_button_press( GtkWidget *widget, GdkEventButton *event )
{
	GtkEnhancedScale * enhanced_scale;
	int i;

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	if ( !GTK_WIDGET_HAS_FOCUS ( widget ) )
		gtk_widget_grab_focus ( widget );
#ifdef DEBUG

	printf( "Received press event, button %d\n", event->button );
#endif

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );
	enhanced_scale->x_click_point = event->x;

	if ( event->window == enhanced_scale->trough )
	{
		gint x, y;
		
#if 0   /* old behaviour to nudge around the sliders by clicking around them. */
		gint n, left, right;
		//gtk_enhanced_scale_get_slider_position( enhanced_scale, &x, &y, enhanced_scale->active_slider );
		gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right,
		                                  enhanced_scale->active_slider );
		n = ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->page_increment )
		    * ( right - left )
		    / ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->upper -
		        GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->lower );

		if ( event->x > x + gtk_enhanced_scale_get_slider_width( widget ) )   /* Click at right of slider */
		{
			gtk_enhanced_scale_motion_x( enhanced_scale, n, enhanced_scale->active_slider );
		}
		else if ( event->x < x )   /* click at left of active slider */
		{
			gtk_enhanced_scale_motion_x( enhanced_scale, -n, enhanced_scale->active_slider );
		}
#endif
#ifdef DEBUG

		printf( "Source: trough. Active slider: %d (pos: %d,%d). Click position: %d,%d.\n",
		        enhanced_scale->active_slider, x, y, ( int ) ( event->x ), ( int ) ( event->y ) );
#endif
		/* New behaviour: a click anywhere not on arrow moves slider 0 to the click spot. */
		gtk_enhanced_scale_get_slider_position(enhanced_scale, &x, &y, 0 );
		gtk_enhanced_scale_motion_x( enhanced_scale, event->x - x - 
			ENHANCED_SCALE_CLASS( enhanced_scale )->arrow_width/2 + 1, 0 );
	}
	else
		for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
		{
			gint x, y;
			gtk_enhanced_scale_get_slider_position( enhanced_scale, &x, &y, i );
			if ( event->window == enhanced_scale->slider[ i ] )
			{
#ifdef DEBUG

				printf( "Source: slider %d (pos: %d,%d)\n", i, x, y );
#endif
				enhanced_scale->active_slider = i;
				enhanced_scale->clicked_slider = i;
				break;
			}
		}
	return TRUE;
}

static gint gtk_enhanced_scale_button_release( GtkWidget *widget, GdkEventButton *event )
{
	GtkEnhancedScale * enhanced_scale;

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

#ifdef DEBUG
	printf( "Button released" );
	if ( enhanced_scale->clicked_slider != -1 )
		printf( " - slider %d unselected", enhanced_scale->clicked_slider );
	printf( "\n" );
#endif

	enhanced_scale->x_click_point = 0;
	enhanced_scale->clicked_slider = -1;
	gtk_widget_queue_draw ( widget );

	return TRUE;
}

static gint gtk_enhanced_scale_motion_notify( GtkWidget *widget, GdkEventMotion *event )
{
	GtkEnhancedScale * enhanced_scale;
	int x, y;
	GdkModifierType mods;

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	if ( enhanced_scale->clicked_slider != -1 )
	{
		gdk_window_get_pointer ( enhanced_scale->slider[ enhanced_scale->clicked_slider ], &x, &y, &mods );
#ifdef DEBUG

		printf( "Motion notify\n" );
#endif

		if ( mods & GDK_BUTTON1_MASK )
			gtk_enhanced_scale_motion_x( enhanced_scale, x - enhanced_scale->x_click_point,
			                             enhanced_scale->clicked_slider );
	}

	return TRUE;
}

static void gtk_enhanced_scale_motion_x( GtkEnhancedScale *enhanced_scale, gint delta_x, gint i )
{
	gdouble old_value;
	gdouble new_value;
	gint left, right;
	gint slider_x, slider_y;
	gint new_pos;

	g_return_if_fail ( enhanced_scale != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( enhanced_scale ) );
	g_return_if_fail ( i >= 0 );
	g_return_if_fail ( i < enhanced_scale->num_adjustments );

	// We don't want the position slider to move
	if (i == 0)
		return;
		
	gdk_window_get_position ( enhanced_scale->slider[ i ], &slider_x, &slider_y );
	gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right, i );
#ifdef DEBUG

	printf( "motion_x called: delta_x = %d, slider_x = %d, l/r=%d/%d\n", delta_x, slider_x, left, right );
#endif

	if ( left == right )
		return ;

	new_pos = slider_x + delta_x;

	if ( new_pos < left )
		new_pos = left;
	else if ( new_pos > right )
		new_pos = right;

	old_value = GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value;
	new_value = ( gint )( ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->upper -
	            GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower -
	            GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->page_size ) *
	          ( new_pos - left ) / ( right - left ) +
	          GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->lower );
	          
	//To prevent in>out
	if (enhanced_scale->num_adjustments >2){
		if (i == 1 && new_value >= GTK_ADJUSTMENT( enhanced_scale->adjustment[ 2 ] ) ->value)
			return;
		if (i == 2 && new_value <= GTK_ADJUSTMENT( enhanced_scale->adjustment[ 1 ] ) ->value)
			return;
	}
	GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value = new_value;
	if ( ( int ) old_value != ( int ) GTK_ADJUSTMENT( enhanced_scale->adjustment[ i ] ) ->value )
	{
		g_signal_emit_by_name ( enhanced_scale->adjustment[ i ], "value_changed" );
	}
}

static gint gtk_enhanced_scale_key_press( GtkWidget *widget, GdkEventKey *event )
{
	GtkEnhancedScale * enhanced_scale;
#ifdef DEBUG

	char *key;
#endif

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );
#ifdef DEBUG

	key = "unkown";
#endif

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	if ( enhanced_scale->active_slider >= 0 && enhanced_scale->active_slider < enhanced_scale->num_adjustments )
	{
		gint n1, n2, left, right;

		gtk_enhanced_scale_trough_hdims ( enhanced_scale, &left, &right,
		                                  enhanced_scale->active_slider );
		n1 = ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->
		       page_increment ) * ( right - left ) /
		     ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->upper -
		       GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->lower );
		n2 = ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->
		       step_increment ) * ( right - left ) /
		     ( GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->upper -
		       GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->lower );

		switch ( event->keyval )
		{
		case GDK_Left:
			if ( event->state & GDK_CONTROL_MASK )
				gtk_enhanced_scale_motion_x( enhanced_scale,
				                             -n1, enhanced_scale->active_slider );
			else
				gtk_enhanced_scale_motion_x( enhanced_scale,
				                             -n2, enhanced_scale->active_slider );

#ifdef DEBUG

			key = "left_arrow";
#endif

			break;
		case GDK_Right:
			if ( event->state & GDK_CONTROL_MASK )
				gtk_enhanced_scale_motion_x( enhanced_scale,
				                             n1, enhanced_scale->active_slider );
			else
				gtk_enhanced_scale_motion_x( enhanced_scale,
				                             n2, enhanced_scale->active_slider );

#ifdef DEBUG

			key = "right_arrow";
#endif

			break;
		case GDK_Home:
			/* Clumsy, but the check in gtk_enhanced_scale_motion_x() will take care */
			gtk_enhanced_scale_motion_x( enhanced_scale,
			                             0 - GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->upper,
			                             enhanced_scale->active_slider );
#ifdef DEBUG

			key = "home";
#endif

			break;
		case GDK_End:
			/* Clumsy, but the check in gtk_enhanced_scale_motion_x() will take care */
			gtk_enhanced_scale_motion_x( enhanced_scale,
			                             GTK_ADJUSTMENT( enhanced_scale->adjustment[ enhanced_scale->active_slider ] ) ->upper,
			                             enhanced_scale->active_slider );
#ifdef DEBUG

			key = "end";
#endif

			break;
		}
	}
#ifdef DEBUG
	printf( "Key pressed: %s\n", key );
#endif

	return TRUE;
}

static gint gtk_enhanced_scale_enter_notify( GtkWidget *widget, GdkEventCrossing *event )
{
	GtkEnhancedScale * enhanced_scale;
	gint i;

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	if ( event->window == enhanced_scale->trough )
	{
		enhanced_scale->in_child = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->trough;
	}
	else
	{
		for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
		{
			if ( event->window == enhanced_scale->slider[ i ] )
			{
				enhanced_scale->in_child = ENHANCED_SCALE_CLASS ( enhanced_scale ) ->slider;
				break;
			}
		}
	}

	return TRUE;
}

static gint gtk_enhanced_scale_leave_notify( GtkWidget *widget, GdkEventCrossing *event )
{
	GtkEnhancedScale * enhanced_scale;

	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );
	enhanced_scale->in_child = 0;
	
	return TRUE;
}

static gint gtk_enhanced_scale_focus_in( GtkWidget *widget, GdkEventFocus *event )
{
	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	GTK_WIDGET_SET_FLAGS ( widget, GTK_HAS_FOCUS );
	//gtk_widget_queue_draw ( widget );
#ifdef DEBUG

	printf( "focus_in\n" );
#endif
	return FALSE;
}

static gint gtk_enhanced_scale_focus_out( GtkWidget *widget, GdkEventFocus *event )
{
	g_return_val_if_fail ( widget != NULL, FALSE );
	g_return_val_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ), FALSE );
	g_return_val_if_fail ( event != NULL, FALSE );

	GTK_WIDGET_UNSET_FLAGS ( widget, GTK_HAS_FOCUS );
	//gtk_widget_queue_draw ( widget );
#ifdef DEBUG

	printf( "focus_out\n" );
#endif
	return FALSE;
}

static void gtk_enhanced_scale_style_set( GtkWidget *widget, GtkStyle *style )
{
	GtkEnhancedScale * enhanced_scale;
	int i;

	g_return_if_fail ( widget != NULL );
	g_return_if_fail ( GTK_IS_ENHANCED_SCALE ( widget ) );

	enhanced_scale = GTK_ENHANCED_SCALE ( widget );

	if ( GTK_WIDGET_REALIZED ( widget ) )
	{
		if ( enhanced_scale->trough )
			gtk_style_set_background ( widget->style, enhanced_scale->trough, GTK_STATE_NORMAL );

		for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
			if ( enhanced_scale->slider[ i ] )
				gtk_style_set_background ( widget->style, enhanced_scale->slider[ i ],
				                           GTK_STATE_NORMAL );
	}
}

static void gtk_enhanced_scale_adjustment_value_changed( GtkAdjustment *adjustment, gpointer data )
{
	GtkEnhancedScale * enhanced_scale;
	gint i;

	g_return_if_fail ( adjustment != NULL );
	g_return_if_fail ( data != NULL );
	

#ifdef DEBUG
	printf( "gtk_enhanced_scale_adjustment_value_changed: %p\n", adjustment );
#endif
	enhanced_scale = GTK_ENHANCED_SCALE( data );
	if (adjustment == GTK_ADJUSTMENT(enhanced_scale->adjustment[0]))
		gtk_enhanced_scale_slider_update(enhanced_scale,0);
	else
		gtk_enhanced_scale_draw ( GTK_WIDGET( enhanced_scale ), NULL );

	return;

    gtk_enhanced_scale_draw_selected_area (enhanced_scale);
	for ( i = 0;i < enhanced_scale->num_adjustments;i++ )
	{
		if ( adjustment == GTK_ADJUSTMENT( enhanced_scale->adjustment[i] ) )
		{
			gtk_enhanced_scale_slider_update( enhanced_scale, i );
			break;
		}
	}
	gtk_enhanced_scale_draw_selected_area (enhanced_scale);
	
}

void gtk_enhanced_scale_set_breaks( GtkWidget *widget, GArray *breaks )
{
	GtkEnhancedScale * enhanced_scale;
	GdkRectangle area;

	g_return_if_fail( widget != NULL );
	g_return_if_fail( breaks != NULL );

	if ( GTK_WIDGET_REALIZED ( widget ) )
    {
		enhanced_scale = GTK_ENHANCED_SCALE( widget );
		enhanced_scale->breaks = breaks;

		gdk_window_clear_area ( widget->window, 0, 0,
	                        widget->allocation.width, widget->allocation.height );

		/* This would be the place to draw it */
		area.x = 0;
		area.y = 0;
		area.width = widget->allocation.width;
		area.height = widget->allocation.height;
		gtk_enhanced_scale_draw ( widget, &area );
	}
}
