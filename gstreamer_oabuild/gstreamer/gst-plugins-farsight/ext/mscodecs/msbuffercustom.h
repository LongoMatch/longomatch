/*
 * Farsight Voice+Video library
 *
 *   @author: Ole Andr� Vadla Ravn�s <oleavr@gmail.com>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License along with this library; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

#ifndef __MSBUFFERCUSTOM_H__
#define __MSBUFFERCUSTOM_H__

#include <glib.h>

#include "msbufferbase.h"

typedef struct {
  MSBufferBase parent;
} MSBufferCustom;

MSBufferCustom * ms_buffer_custom_new (guint8 * buf, guint buf_size);

#endif /* __MSBUFFERCUSTOM_H__ */
