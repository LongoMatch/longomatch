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

#include "gtk-timescale.h"
#include <malloc.h>

#define ADJ_POS 0
#define ADJ_IN 1
#define ADJ_OUT 2


/* Signals */
enum
{
  SIGNAL_POS_CHANGED,
  SIGNAL_IN_CHANGED,
  SIGNAL_OUT_CHANGED,
  LAST_SIGNAL
  
};



static gboolean on_trim_value_changed_event  ( 	GtkAdjustment *adj, gpointer user_data );
static void gtk_timescale_signals_connect(GtkTimescale *gts);
static void gtk_timescale_signals_disconnect(GtkTimescale *gts);

static int gts_signals[LAST_SIGNAL] = { 0 };

G_DEFINE_TYPE (GtkTimescale, gtk_timescale, GTK_TYPE_HBOX);

static void
gtk_timescale_init (GtkTimescale *gts)
{
	
	GtkWidget *scale;	
	int i;
	                 
                  
		// Attach the custom GTK+ Trim control
	gts->adjustment[ ADJ_POS ] = GTK_ADJUSTMENT(
	                               gtk_adjustment_new( 0, 0, 0, 1, 10, 0 ) );
	gts->adjustment[ ADJ_IN ] = GTK_ADJUSTMENT(
	                              gtk_adjustment_new( 0, 0, 0, 1, 10, 0 ) );
	gts->adjustment[ ADJ_OUT ] = GTK_ADJUSTMENT(
	                               gtk_adjustment_new( 0, 0, 0, 1, 10, 0 ) );
	                               
	gtk_timescale_signals_connect(gts);
	
	scale = gtk_enhanced_scale_new( ( GtkObject** ) gts->adjustment, 3 );
	
	gtk_container_add(GTK_CONTAINER(gts),scale);
	gtk_widget_show (scale);
	
	
}

static void 
gtk_timescale_signals_connect(GtkTimescale *gts)
{
	int i = 0;
	gts->handler_id = malloc( 3 * sizeof(*gts->handler_id) );
	for (i = 0; i < 3; i++ ){
		gts->handler_id[ i ] =g_signal_connect ( gts->adjustment[ i ] , "value_changed",
		                   G_CALLBACK( on_trim_value_changed_event ),   gts );
	
	}
	
}

static void 
gtk_timescale_signals_disconnect(GtkTimescale *gts)
{
	int i = 0;
	for ( i = 0;i < 3 ;i++ )
		gtk_signal_disconnect( gts->adjustment[ i ], gts->handler_id[ i ] );
}

static void
gtk_timescale_finalize (GObject *object)
{
	/* TODO: Add deinitalization code here */
	GtkTimescale * time_scale;
	gint i;

	g_return_if_fail ( object != NULL );
	g_return_if_fail ( GTK_IS_TIMESCALE( object ) );

	time_scale = GTK_TIMESCALE ( object );
	
	
	/*for ( i = 0;i < 3 ;i++ )
		gtk_signal_disconnect( time_scale->adjustment[ i ], time_scale->handler_id[ i ] );*/

	G_OBJECT_CLASS (gtk_timescale_parent_class)->finalize (object);
}

static void
gtk_timescale_class_init (GtkTimescaleClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);
	GtkHBoxClass* parent_class = GTK_HBOX_CLASS (klass);

	object_class->finalize = gtk_timescale_finalize;
	
	/* Signals */
 	gts_signals[SIGNAL_POS_CHANGED] =
    g_signal_new ("pos_changed",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GtkTimescaleClass, pos_changed),
                  NULL, NULL, g_cclosure_marshal_VOID__DOUBLE, 
                  G_TYPE_NONE, 1, G_TYPE_DOUBLE);

  	gts_signals[SIGNAL_IN_CHANGED] =
    g_signal_new ("in_changed",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GtkTimescaleClass, in_changed),
                  NULL, NULL, g_cclosure_marshal_VOID__DOUBLE, 
                  G_TYPE_NONE, 1, G_TYPE_DOUBLE);
  
  	gts_signals[SIGNAL_OUT_CHANGED] =
    g_signal_new ("out_changed",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GtkTimescaleClass, out_changed),
                  NULL, NULL, g_cclosure_marshal_VOID__DOUBLE, 
                  G_TYPE_NONE, 1, G_TYPE_DOUBLE);
}


static gboolean
on_trim_value_changed_event ( GtkAdjustment * adj, gpointer user_data )
	{
		
		gdouble value;
		GtkTimescale *gts;
		
		
		
		g_return_val_if_fail ( user_data !=  NULL , FALSE);
		g_return_val_if_fail ( GTK_TIMESCALE (user_data), FALSE);
		
		gts = GTK_TIMESCALE (user_data);

		value = adj->value;

		
		if ( adj == gts->adjustment[ ADJ_IN ] ){
			g_signal_emit (gts,gts_signals[SIGNAL_IN_CHANGED], 0, value);			
		}
		if ( adj == gts->adjustment[ ADJ_OUT ] ){
			g_signal_emit (gts, gts_signals[SIGNAL_OUT_CHANGED], 0, value);
		}
		if ( adj == gts->adjustment[ ADJ_POS ] ){
			g_signal_emit (gts, gts_signals[SIGNAL_POS_CHANGED], 0, value);
		}
		return FALSE;
	}

GtkWidget *
gtk_timescale_new (gint32 upper)
{
	GtkTimescale *widget = g_object_new (GTK_TYPE_TIMESCALE , NULL);
 	widget->adjustment[ADJ_POS]->upper = upper;
	widget->adjustment[ADJ_IN]->upper = upper;
	widget->adjustment[ADJ_OUT]->upper = upper;

 	return GTK_WIDGET (widget);
}
void 
gtk_timescale_set_bounds(GtkTimescale *gts, gdouble lower, gdouble upper)
{
	int i=0;
	
	g_return_if_fail (lower < upper);
	

	for (i = 0; i < 3; i++ ){
		gts->adjustment[i]->lower = lower;
		gts->adjustment[i]->upper = upper;
		
	}
	
	// Solo queremos que se redibuje la escala
	gtk_timescale_signals_disconnect(gts);
	gtk_adjustment_value_changed	(gts->adjustment[ADJ_IN]);
	gtk_timescale_signals_connect(gts);
	

	
}

void gtk_timescale_adjust_position(GtkTimescale *gts,gdouble val, gint adj){
	g_return_if_fail (val > gts->adjustment[adj]->lower || val < gts->adjustment[adj]->upper);
	
	// Este método se ejecuta cuando a sido notificado un cambio de posición.ype
	
	// La desconexión de la señal "changed_value" permite que no entremos en un loop en el
	// ajuste de posición. De no ser así al hacer una ajuste de posicón manual se emitiría 
	// una señal "value_chanded" que nos podría llevar a realizar otro nuevo ajuste de 
	// posición
	gtk_timescale_signals_disconnect(gts);
	gtk_adjustment_set_value( gts->adjustment[adj], val );
	gtk_timescale_signals_connect(gts);
}

	
	
void
gtk_timescale_set_segment(GtkTimescale * gts, gdouble in, gdouble out)
{
	
	
	g_return_if_fail ( in<out );
	
	// No queremos que se notifiquen cambios de posición al ajustar nuevos valores 
	// de segmento, para no crear confusión al lanzar un cambio de posición a in
	// otro a out y de nuevo a in al iniciar la reproducción desde ese punto
	gtk_timescale_signals_disconnect(gts);
	
	gtk_adjustment_set_value( gts->adjustment[ADJ_IN], in );
	gtk_adjustment_set_value( gts->adjustment[ADJ_OUT], out);
	gtk_adjustment_set_value( gts->adjustment[ADJ_POS],in);
	
	gtk_timescale_signals_connect(gts);
	
	
}
	
	
	

